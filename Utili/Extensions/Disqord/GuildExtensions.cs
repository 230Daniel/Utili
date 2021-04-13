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

        public static CachedCategoryChannel GetCategoryChannel(this CachedGuild guild, Snowflake channelId)
        {
            return guild.GetChannel(channelId) as CachedCategoryChannel;
        }

        public static bool BotHasPermissions(this IGuild guild, DiscordClientBase client, params Permission[] requiredPermissions)
        {
            CachedGuild cachedGuild = client.GetGuild(guild.Id);
            IMember bot = cachedGuild.Members.GetValueOrDefault(client.CurrentUser.Id);
            List<CachedRole> roles = bot.GetRoles().Values.ToList();

            GuildPermissions permissions = Disqord.Discord.Permissions.CalculatePermissions(cachedGuild, bot, roles);
            return requiredPermissions.All(x => permissions.Contains(x));
        }

        public static bool BotHasPermissions(this CachedGuild guild, DiscordClientBase client, out string missingPermissions, params Permission[] requiredPermissions)
        {
            CachedGuild cachedGuild = client.GetGuild(guild.Id);
            IMember bot = cachedGuild.Members.GetValueOrDefault(client.CurrentUser.Id);
            List<CachedRole> roles = bot.GetRoles().Values.ToList();

            GuildPermissions permissions = Disqord.Discord.Permissions.CalculatePermissions(cachedGuild, bot, roles);

            missingPermissions = "";

            return requiredPermissions.All(x => permissions.Contains(x));
        }
    }
}
