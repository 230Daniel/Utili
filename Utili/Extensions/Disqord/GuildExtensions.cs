using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;

namespace Utili.Extensions
{
    static class GuildExtensions
    {
        public static CachedTextChannel GetTextChannel(this CachedGuild guild, Snowflake channelId)
        {
            return guild.GetChannel(channelId) as CachedTextChannel;
        }

        public static CachedVoiceChannel GetVoiceChannel(this CachedGuild guild, Snowflake channelId)
        {
            return guild.GetChannel(channelId) as CachedVoiceChannel;
        }
    }
}
