using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewDatabase;

namespace DatabaseStressTest
{
    public class Program
    {
        public static async Task Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();
            
            try
            {
                using (host)
                {
                    await host.RunAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }
        
        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddHostedService<StressTestService>();
            services.AddScoped<Worker>();
            services.AddSingleton<Random>();
            services.AddDbContext<DatabaseContext>();
            services.AddLogging();
        }
    }
}
