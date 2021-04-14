using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;

namespace Utili.Extensions
{
    static class ClientExtensions
    {
        public static CachedTextChannel GetTextChannel(this DiscordClientBase client, Snowflake guildId, Snowflake channelId)
        {
            return client.GetChannel(guildId, channelId) as CachedTextChannel;
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
