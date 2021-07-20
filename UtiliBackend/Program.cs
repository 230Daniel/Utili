using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UtiliBackend.Services;

namespace UtiliBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddProvider(new LoggerProvider());
                })
                .Build();
            
            try
            {
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Critical failure");
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }
    }
}
