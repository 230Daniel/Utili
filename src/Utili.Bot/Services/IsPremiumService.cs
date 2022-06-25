using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utili.Bot.Extensions;

namespace Utili.Bot.Services;

public class IsPremiumService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public bool IsFree { get; }

    public IsPremiumService(IConfiguration config, IServiceScopeFactory scopeFactory)
    {
        IsFree = !config.GetValue<bool>("Services:Premium");
        _scopeFactory = scopeFactory;
    }

    public async Task<bool> GetIsGuildPremiumAsync(ulong guildId)
    {
        if (IsFree) return true;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.GetDbContext();

        return await db.PremiumSlots.AnyAsync(x => x.GuildId == guildId);
    }
}
