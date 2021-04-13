using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;

namespace Utili.Extensions
{
    static class ChannelExtensions
    {
        public static bool BotHasPermissions(this IGuildChannel channel, DiscordClientBase client, params Permission[] requiredPermissions)
        {
            CachedGuild guild = client.GetGuild(channel.GuildId);
            IMember bot = guild.Members.GetValueOrDefault(client.CurrentUser.Id);
            List<CachedRole> roles = bot.GetRoles().Values.ToList();

            ChannelPermissions permissions = Disqord.Discord.Permissions.CalculatePermissions(guild, channel, bot, roles);
            return requiredPermissions.All(x => permissions.Contains(x));
        }

        public static bool BotHasPermissions(this IGuildChannel channel, DiscordClientBase client, out string missingPermissions, params Permission[] requiredPermissions)
        {
            CachedGuild guild = client.GetGuild(channel.GuildId);
            IMember bot = guild.Members.GetValueOrDefault(client.CurrentUser.Id);
            IEnumerable<CachedRole> roles = bot.GetRoles().Values;

            ChannelPermissions permissions = Disqord.Discord.Permissions.CalculatePermissions(guild, channel, bot, roles);

            missingPermissions = "";

            return requiredPermissions.All(x => permissions.Contains(x));
        }
    }
}
