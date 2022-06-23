using System;
using System.Threading.Tasks;
using Disqord;
using Microsoft.Extensions.DependencyInjection;
using Database;
using Database.Entities;
using Utili.Services;

namespace Utili.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static DatabaseContext GetDbContext(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<DatabaseContext>();
        }

        public static Task<CoreConfiguration> GetCoreConfigurationAsync(this IServiceProvider serviceProvider, Snowflake guildId)
        {
            var service = serviceProvider.GetRequiredService<CoreConfigurationCacheService>();
            return service.GetCoreConfigurationAsync(guildId);
        }
    }
}
