using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewDatabase;
using Database.Data;
using Microsoft.EntityFrameworkCore;
using NewDatabase.Entities;

namespace DatabaseMigrator.Services
{
    
    public class MigratorService
    {
        private static readonly bool UseProdDb = false;
        
        private readonly ILogger<MigratorService> _logger;
        private readonly DatabaseContext _db;

        public MigratorService(ILogger<MigratorService> logger, DatabaseContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task RunAsync()
        {
            try
            {
                await Database.Database.InitialiseAsync(false, ".", UseProdDb);
                _logger.LogInformation("Old database initialised - Prod: {Prod}", UseProdDb);

                _logger.LogInformation("Migrating autopurge...");
                await MigrateAutopurgeAsync();
                
                _logger.LogInformation("Finished");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while running");
            }
        }

        private async Task MigrateAutopurgeAsync()
        {
            static AutopurgeMode GetMode(int oldMode)
            {
                return oldMode switch
                {
                    -1 => AutopurgeMode.User,
                    0 => AutopurgeMode.All,
                    1 => AutopurgeMode.Bot,
                    2 => AutopurgeMode.None,
                    3 => AutopurgeMode.User,
                    _ => throw new ArgumentException($"Bad autopurge mode {oldMode}", nameof(oldMode))
                };
            }
            
            var rows = await Autopurge.GetRowsAsync();
            _db.AutopurgeConfigurations.RemoveRange(await _db.AutopurgeConfigurations.ToListAsync());
            
            foreach (var row in rows)
            {
                var autopurgeConfiguration = new AutopurgeConfiguration(row.GuildId, row.ChannelId)
                {
                    Mode = GetMode(row.Mode),
                    Timespan = row.Timespan
                };
                _db.AutopurgeConfigurations.Add(autopurgeConfiguration);
                _logger.LogDebug("Migrated autopurge configuration {GuildId}/{ChannelId}", row.GuildId, row.ChannelId);
            }

            /*
            var messageRows = await Autopurge.GetMessagesAsync();
            _db.AutopurgeMessages.RemoveRange(await _db.AutopurgeMessages.ToListAsync());
            
            foreach (var messageRow in messageRows.Where(x => !messageKeys.Contains(x.MessageId)))
            {
                var autopurgeMessage = new AutopurgeMessage(messageRow.MessageId)
                {
                    GuildId = messageRow.GuildId,
                    ChannelId = messageRow.ChannelId,
                    Timestamp = messageRow.Timestamp,
                    IsBot = messageRow.IsBot,
                    IsPinned = messageRow.IsPinned
                };
                _db.AutopurgeMessages.Add(autopurgeMessage);
                _logger.LogDebug("Migrated autopurge message {MessageId}", messageRow.MessageId);
            }
            */

            await _db.SaveChangesAsync();
        }
    }
}
