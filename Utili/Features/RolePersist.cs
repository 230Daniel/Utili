using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Database.Data;

namespace Utili.Features
{
    internal static class RolePersist
    {
        public static async Task UserJoined(SocketGuildUser user)
        {
            SocketGuild guild = user.Guild;
            RolePersistRow row = await Database.Data.RolePersist.GetRowAsync(guild.Id);
            if (!row.Enabled) return;
            
            RolePersistRolesRow persistRow = await Database.Data.RolePersist.GetPersistRowAsync(guild.Id, user.Id);

            foreach (ulong roleId in persistRow.Roles.Distinct().Where(x => !row.ExcludedRoles.Contains(x)))
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

            await Database.Data.RolePersist.DeletePersistRowAsync(persistRow);
        }

        public static async Task UserLeft(SocketGuildUser user)
        {
            SocketGuild guild = user.Guild;
            RolePersistRow row = await Database.Data.RolePersist.GetRowAsync(guild.Id);
            if(!row.Enabled) return;
            
            RolePersistRolesRow persistRow = await Database.Data.RolePersist.GetPersistRowAsync(guild.Id, user.Id);
            persistRow.Roles.AddRange(user.Roles.Where(x => !x.IsEveryone).Select(x => x.Id));

            await Database.Data.RolePersist.SavePersistRowAsync(persistRow);
        }
    }
}
