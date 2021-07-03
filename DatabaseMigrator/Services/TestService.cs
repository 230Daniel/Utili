using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewDatabase;
using NewDatabase.Entities;

namespace DatabaseMigrator.Services
{
    public class TestService
    {
        private readonly ILogger<TestService> _logger;
        private readonly DatabaseContext _db;

        public TestService(ILogger<TestService> logger, DatabaseContext db)
        {
            _logger = logger;
            _db = db;
        }
        
        public async Task RunAsync()
        {
            await Task.Delay(2000);

            try
            {
                var entity = new TestEntity(790255755524571157, 790255761485201421)
                {
                    Value = "Hello world!"
                };

                await _db.TestEntities.AddAsync(entity);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Done");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown");
            }
            
            try
            {
                var entity = await _db.TestEntities.FirstOrDefaultAsync(x => x.GuildId == 790255755524571157 && x.ChannelId == 790255761485201421);
                
                _logger.LogInformation("{GuildId} {ChannelId} {Value}", entity.GuildId, entity.ChannelId, entity.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown");
            }

            
        }
    }
}
