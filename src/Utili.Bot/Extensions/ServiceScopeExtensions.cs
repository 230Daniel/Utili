using System.Threading.Tasks;
using Disqord;
using Microsoft.Extensions.DependencyInjection;
using Utili.Database;
using Utili.Database.Entities;

namespace Utili.Bot.Extensions;

public static class ServiceScopeExtensions
{
    public static Task<CoreConfiguration> GetCoreConfigurationAsync(this IServiceScope scope, Snowflake guildId)
    {
        return scope.ServiceProvider.GetCoreConfigurationAsync(guildId);
    }

    public static DatabaseContext GetDbContext(this IServiceScope scope)
    {
        return scope.ServiceProvider.GetDbContext();
    }
}