using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NewDatabase.Extensions
{
    public static class DbContextExtensions
    {
        public static Task<bool> GetIsGuildPremiumAsync(this DatabaseContext dbContext, ulong guildId)
        {
            return dbContext.PremiumSlots.AnyAsync(x => x.GuildId == guildId);
        }
    }
}
