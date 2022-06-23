using Disqord;
using Disqord.Gateway;

namespace Utili.Extensions
{
    public static class ClientExtensions
    {
        public static CachedTextChannel GetTextChannel(this DiscordClientBase client, Snowflake guildId, Snowflake channelId)
        {
            return client.GetChannel(guildId, channelId) as CachedTextChannel;
        }

        public static CachedMessageGuildChannel GetMessageGuildChannel(this DiscordClientBase client, Snowflake guildId, Snowflake channelId)
        {
            return client.GetChannel(guildId, channelId) as CachedMessageGuildChannel;
        }

        public static CachedVoiceChannel GetVoiceChannel(this DiscordClientBase client, Snowflake guildId, Snowflake channelId)
        {
            return client.GetChannel(guildId, channelId) as CachedVoiceChannel;
        }

        public static CachedCategoryChannel GetCategoryChannel(this DiscordClientBase client, Snowflake guildId, Snowflake channelId)
        {
            return client.GetChannel(guildId, channelId) as CachedCategoryChannel;
        }
    }
}
