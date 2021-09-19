using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DatabaseMigrator.Services
{
    public class DatabaseMigratorService : IHostedService
    {
        private readonly MigratorService _migratorService;

        public DatabaseMigratorService(MigratorService migratorService)
        {
            _migratorService = migratorService;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _ = _migratorService.RunAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            
        }
    }
}
