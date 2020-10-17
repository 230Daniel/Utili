using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static bool IsMissingPermissions(SocketGuildChannel channel, ChannelPermission[] requiredPermissions, out string missingPermissionsString)
        {
            SocketGuildUser bot = channel.Guild.GetUser(_client.CurrentUser.Id);

            List<ChannelPermission> permissions = bot.GetPermissions(channel).ToList();

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
