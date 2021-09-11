using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewDatabase;
using NewDatabase.Extensions;

namespace DatabaseStressTest
{
    public class Worker
    {
        private readonly ILogger<Worker> _logger;
        private readonly DatabaseContext _db;
        private readonly Random _random;

        public int Requests { get; protected set; }

        public Worker(ILogger<Worker> logger, DatabaseContext db, Random random)
        {
            _logger = logger;
            _db = db;
            _random = random;
        }
        
        public async Task RunAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var milliseconds = _random.Next(0, 2147483647);
                    var randomId = Snowflake.FromDateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(milliseconds));

                    await _db.CoreConfigurations.GetForGuildAsync(randomId);
                    Requests++;
                    _logger.LogDebug("Request made with ID {ID}", randomId.RawValue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker threw an exception and terminated");
            }
        }
    }
}
