using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili
{
    internal static class BotPermissions
    {
        public static bool IsMissingPermissions(SocketGuild guild, GuildPermission[] requiredPermissions, out string missingPermissionsString)
        {
            SocketGuildUser bot = guild.GetUser(_oldClient.CurrentUser.Id);

            List<GuildPermission> permissions = bot.GuildPermissions.ToList();

            List<GuildPermission> missingPermissions =
                requiredPermissions.Where(x => !permissions.Contains(x)).ToList();

            if (missingPermissions.Count == 0)
            {
                missingPermissionsString = null;
                return false;
            }

            missingPermissionsString = "";
            for (int i = 0; i < missingPermissions.Count; i++)
            {
                GuildPermission missingPermission = missingPermissions[i];

                if (i == missingPermissions.Count - 1)
                {
                    missingPermissionsString += $"{missingPermission}";
                }

                else if (i == missingPermissions.Count - 2)
                {
                    missingPermissionsString += $"{missingPermission} and ";
                }

                else
                {
                    missingPermissionsString += $"{missingPermission}, ";
                }
            }

            return true;
        }

        public static bool IsMissingPermissions(IChannel channel, ChannelPermission[] requiredPermissions, out string missingPermissionsString)
        {
            missingPermissionsString = null;
            if(channel is null) return false;

            SocketGuild guild = (channel as SocketGuildChannel).Guild;
            SocketGuildUser bot = guild.GetUser(_oldClient.CurrentUser.Id);

            List<ChannelPermission> permissions = bot.GetPermissions(channel as IGuildChannel).ToList();

            List<ChannelPermission> missingPermissions =
                requiredPermissions.Where(x => !permissions.Contains(x)).ToList();

            if (missingPermissions.Count == 0)
            {
                return false;
            }

            missingPermissionsString = "";
            for (int i = 0; i < missingPermissions.Count; i++)
            {
                ChannelPermission missingPermission = missingPermissions[i];

                if (i == missingPermissions.Count - 1)
                {
                    missingPermissionsString += $"{missingPermission}";
                }

                else if (i == missingPermissions.Count - 2)
                {
                    missingPermissionsString += $"{missingPermission} and ";
                }

                else
                {
                    missingPermissionsString += $"{missingPermission}, ";
                }
            }

            return true;
        }

        public static bool BotHasPermissions(this IGuildChannel channel, params ChannelPermission[] requiredPermissions)
        {
            SocketGuild guild = _oldClient.GetGuild(channel.GuildId);
            SocketGuildUser bot = guild.GetUser(_oldClient.CurrentUser.Id);

            List<ChannelPermission> permissions = bot.GetPermissions(channel).ToList();

            return requiredPermissions.All(x => permissions.Contains(x));
        }

        public static bool BotHasPermissions(this SocketGuild guild, params GuildPermission[] requiredPermissions)
        {
            SocketGuildUser bot = guild.GetUser(_oldClient.CurrentUser.Id);
            List<GuildPermission> permissions = bot.GuildPermissions.ToList();

            return requiredPermissions.All(x => permissions.Contains(x));
        }

        public static bool CanManageRole(SocketRole role)
        {
            SocketGuild guild = role.Guild;
            SocketGuildUser bot = guild.GetUser(_oldClient.CurrentUser.Id);

            if (!guild.BotHasPermissions(GuildPermission.ManageRoles)) return false;

            int highestPossiblePosition = bot.Roles.Max(x => x.Position) - 1;

            return role.Position <= highestPossiblePosition;
        }
    }
}
