using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Gateway;

namespace Utili.Extensions
{
    static class ChannelExtensions
    {
        public static bool BotHasPermissions(this IGuildChannel channel, DiscordClientBase client, Permission permissions)
        {
            return channel.GetGuild(client).GetCurrentMember(client).GetChannelPermissions(channel).Has(permissions);
        }

        public static IGuild GetGuild(this IGuildChannel channel, DiscordClientBase client)
        {
            return client.GetGuild(channel.GuildId);
        }
    }
}
