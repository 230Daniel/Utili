using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Utili.Database;

namespace Utili.Backend.Services;

public class IsPremiumService
{
    private readonly DatabaseContext _dbContext;
    public bool IsFree { get; }

    public IsPremiumService(IConfiguration config, DatabaseContext dbContextContext)
    {
        IsFree = !config.GetValue<bool>("Stripe:Enable");
        _dbContext = dbContextContext;
    }

    public async Task<bool> GetIsGuildPremiumAsync(ulong guildId)
    {
        if (IsFree) return true;

        return await _dbContext.PremiumSlots.AnyAsync(x => x.GuildId == guildId);
    }
}
