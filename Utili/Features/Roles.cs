using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Database.Data;
using static Utili.Program;

namespace Utili.Features
{
    internal class Roles
    {
        public async Task UserJoined(SocketGuildUser user)
        {
            SocketGuild guild = user.Guild;

            List<RolesRow> rows = Database.Data.Roles.GetRows(guild.Id);
            if(rows.Count == 0) return;
            RolesRow row = rows.First();

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
                        await user.AddRoleAsync(role);
                        await Task.Delay(1000);
                    }
                }

                if (guild.Users.Select(x => x.Id).Contains(user.Id))
                {
                    foreach (RolesPersistantRolesRow persistRow in persistRows)
                    {
                        Database.Data.Roles.DeletePersistRow(persistRow);
                    }
                }
            }
        }

        // TODO: Download users for guilds using this feature
        // Therefore it should be premium??

        public async Task UserLeft(SocketGuildUser user)
        {
            RolesPersistantRolesRow persistRow = new RolesPersistantRolesRow
            {
                GuildId = user.Guild.Id,
                UserId = user.Id,
                Roles = user.Roles.Select(x => x.Id).ToList()
            };

            Database.Data.Roles.SavePersistRow(persistRow);
        }
    }
}
