using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using static Utili.Program;

namespace Utili
{
    internal class BotPermissions
    {
        public static bool IsMissingPermissions(SocketGuild guild, GuildPermission[] requiredPermissions, out string missingPermissionsString)
        {
            SocketGuildUser bot = guild.GetUser(_client.CurrentUser.Id);

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
            SocketGuild guild = (channel as SocketGuildChannel).Guild;
            SocketGuildUser bot = guild.GetUser(_client.CurrentUser.Id);

            List<ChannelPermission> permissions = bot.GetPermissions(channel as ITextChannel).ToList();

            List<ChannelPermission> missingPermissions =
                requiredPermissions.Where(x => !permissions.Contains(x)).ToList();

            if (missingPermissions.Count == 0)
            {
                missingPermissionsString = null;
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
    }
}
