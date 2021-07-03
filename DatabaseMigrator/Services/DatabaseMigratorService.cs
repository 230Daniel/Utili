using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DatabaseMigrator.Services
{
    public class DatabaseMigratorService : IHostedService
    {
        private readonly TestService _testService;

        public DatabaseMigratorService(TestService testService)
        {
            _testService = testService;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _ = _testService.RunAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            
        }
    }
}
