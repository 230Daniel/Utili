using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Database.Data;

namespace Utili.Features
{
    internal static class Roles
    {
        public static async Task UserJoined(SocketGuildUser user)
        {
            SocketGuild guild = user.Guild;

            RolesRow row = Database.Data.Roles.GetRow(guild.Id);

            foreach (ulong roleId in row.JoinRoles)
            {
                SocketRole role = guild.GetRole(roleId);
                if (role != null && BotPermissions.CanManageRole(role))
                {
                    await user.AddRoleAsync(role);
                    await Task.Delay(1000);
                }
            }

            if (row.RolePersist)
            {
                List<RolesPersistantRolesRow> persistRows = Database.Data.Roles.GetPersistRows(guild.Id, user.Id);
                List<ulong> roleIds = persistRows.SelectMany(x => x.Roles).Distinct().ToList();

                foreach (ulong roleId in roleIds)
                {
                    SocketRole role = guild.GetRole(roleId);
                    if (role != null && BotPermissions.CanManageRole(role))
                    {
                        try
                        {
                            await user.AddRoleAsync(role);
                            await Task.Delay(1000);
                        }
                        catch { }
                    }
                }

                if (guild.Users.Any(x => x.Id == user.Id))
                {
                    foreach (RolesPersistantRolesRow persistRow in persistRows)
                    {
                        Database.Data.Roles.DeletePersistRow(persistRow);
                    }
                }
            }
        }

        public static async Task UserLeft(SocketGuildUser user)
        {
            SocketGuild guild = user.Guild;

            RolesRow row = Database.Data.Roles.GetRow(guild.Id);

            if (row.RolePersist)
            {
                RolesPersistantRolesRow persistRow = new RolesPersistantRolesRow
                {
                    GuildId = user.Guild.Id,
                    UserId = user.Id,
                    Roles = user.Roles.Where(x => !x.IsEveryone).Select(x => x.Id).ToList()
                };

                Database.Data.Roles.SavePersistRow(persistRow);
            }
        }
    }
}
