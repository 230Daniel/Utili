using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;

namespace Utili.Extensions
{
    public static class GuildExtensions
    {
        public static CachedTextChannel GetTextChannel(this IGuild guild, Snowflake channelId)
        {
            return guild.GetChannel(channelId) as CachedTextChannel;
        }
        
        public static CachedMessageGuildChannel GetMessageGuildChannel(this IGuild guild, Snowflake channelId)
        {
            return guild.GetChannel(channelId) as CachedMessageGuildChannel;
        }

        public static CachedVoiceChannel GetVoiceChannel(this IGuild guild, Snowflake channelId)
        {
            return guild.GetChannel(channelId) as CachedVoiceChannel;
        }
        
        public static CachedVocalGuildChannel GetAudioChannel(this IGuild guild, Snowflake channelId)
        {
            return guild.GetChannel(channelId) as CachedVocalGuildChannel;
        }

        public static CachedCategoryChannel GetCategoryChannel(this IGuild guild, Snowflake channelId)
        {
            return guild.GetChannel(channelId) as CachedCategoryChannel;
        }

        public static IRole GetRole(this IGuild guild, Snowflake roleId)
        {
            return guild.Roles.Values.FirstOrDefault(x => x.Id == roleId);
        }

        public static IMember GetCurrentMember(this IGuild guild)
        {
            return guild.GetMember((guild.Client as DiscordClientBase).CurrentUser.Id);
        }

        public static bool BotHasPermissions(this IGuild guild, Permission permissions)
        {
            return guild.GetCurrentMember().GetPermissions().Has(permissions);
        }

        public static IEmoji GetEmoji(this IGuild guild, string emojiString)
        {
            var guildEmoji = guild.Emojis.Values.FirstOrDefault(x => x.Tag == emojiString || $":{x.Name}:" == emojiString);
            return guildEmoji is null
                ? emojiString.Contains("<") ? null : new LocalEmoji(emojiString)
                : guildEmoji;
        }

        public static ValueTask<bool> ChunkMembersAsync(this IGatewayGuild guild)
        {
            return (guild.Client as DiscordClientBase).Chunker.ChunkAsync(guild);
        }
    }
}
