using System;
using System.Threading.Tasks;
using Disqord;
using Microsoft.Extensions.DependencyInjection;
using Utili.Database;
using Utili.Database.Entities;
using Utili.Bot.Services;

namespace Utili.Bot.Extensions;

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