using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewDatabase.Entities.Base;

namespace NewDatabase.Extensions
{
    public static class DbSetExtensions
    {
        public static Task<T> GetForGuildAsync<T>(this DbSet<T> dbSet, ulong guildId) where T : GuildEntity
        {
            return dbSet.FirstOrDefaultAsync(x => x.GuildId == guildId);
        }
        
        public static Task<T> GetForGuildChannelAsync<T>(this DbSet<T> dbSet, ulong guildId, ulong channelId) where T : GuildChannelEntity
        {
            return dbSet.FirstOrDefaultAsync(x => x.GuildId == guildId && x.ChannelId == channelId);
        }
        
        public static Task<T> GetForMessageAsync<T>(this DbSet<T> dbSet, ulong messageId) where T : MessageEntity
        {
            return dbSet.FirstOrDefaultAsync(x => x.MessageId == messageId);
        }
    }
}
