using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Database;

namespace UtiliBackend.Services
{
    public class SlotDeletionService : IHostedService
    {
        private readonly ILogger<SlotDeletionService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        
        private Task _task;
        private CancellationTokenSource _tokenSource;

        public SlotDeletionService(ILogger<SlotDeletionService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _tokenSource = new CancellationTokenSource();
            _task = RunAsync(_tokenSource.Token);
            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _tokenSource.Cancel();
            return _task;
        }
        
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await DeleteAdditionalSlotsAsync(cancellationToken);
                    await Task.Delay(30000, cancellationToken);
                }
            }
            catch (TaskCanceledException) 
            { }
        }
        
        private async Task DeleteAdditionalSlotsAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var slots = await db.PremiumSlots.ToListAsync(cancellationToken);
                var subscriptions = await db.Subscriptions.ToListAsync(cancellationToken);
                subscriptions.RemoveAll(x => !x.IsValid());

                var userIds = slots.Select(x => x.UserId).Distinct();
                foreach (var userId in userIds)
                {
                    var userSlots = slots.Where(x => x.UserId == userId).OrderBy(x => x.SlotId);

                    var allowedSlotCount = subscriptions.Where(x => x.UserId == userId).Sum(x => x.Slots);
                    var actualSlotCount = userSlots.Count();

                    if (actualSlotCount > allowedSlotCount)
                    {
                        var extraSlots = userSlots.Skip(allowedSlotCount);
                        db.PremiumSlots.RemoveRange(extraSlots);
                    }
                }

                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not TaskCanceledException)
            {
                _logger.LogError(ex, "Exception thrown while deleting excess premium slots");
            }
        }
    }
}
