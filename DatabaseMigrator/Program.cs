using System;
using System.Threading.Tasks;
using DatabaseMigrator.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewDatabase;

namespace DatabaseMigrator
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(builder => 
                    builder.AddCommandLine(args))
                .ConfigureServices(ConfigureServices)
                .Build();

            try
            {
                using (var scope = host.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    await db.Database.MigrateAsync();
                }
                    
                await host.RunAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Critical failure");
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHostedService<DatabaseMigratorService>();
            services.AddTransient<MigratorService>();
            services.AddDbContext<DatabaseContext>();
        }
    }
}
