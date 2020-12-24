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

            RolesRow row = await Database.Data.Roles.GetRowAsync(guild.Id);

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
                RolesPersistantRolesRow persistRow = await Database.Data.Roles.GetPersistRowAsync(guild.Id, user.Id);

                foreach (ulong roleId in persistRow.Roles.Distinct())
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
                    if(guild.Users.All(x => x.Id != user.Id)) return;
                }

                await Database.Data.Roles.DeletePersistRowAsync(persistRow);
            }
        }

        public static async Task UserLeft(SocketGuildUser user)
        {
            SocketGuild guild = user.Guild;
            RolesRow row = await Database.Data.Roles.GetRowAsync(guild.Id);

            if (row.RolePersist)
            {
                RolesPersistantRolesRow persistRow = await Database.Data.Roles.GetPersistRowAsync(guild.Id, user.Id);
                persistRow.Roles.AddRange(user.Roles.Where(x => !x.IsEveryone).Select(x => x.Id));

                await Database.Data.Roles.SavePersistRowAsync(persistRow);
            }
        }
    }
}
