using System.Threading.Tasks;
using Discord.WebSocket;
using Database.Data;

namespace Utili.Features
{
    internal static class JoinRoles
    {
        public static async Task UserJoined(SocketGuildUser user)
        {
            SocketGuild guild = user.Guild;
            JoinRolesRow row = await Database.Data.JoinRoles.GetRowAsync(guild.Id);

            foreach (ulong roleId in row.JoinRoles)
            {
                SocketRole role = guild.GetRole(roleId);
                if (role != null && BotPermissions.CanManageRole(role))
                {
                    await user.AddRoleAsync(role);
                    await Task.Delay(1000);
                }
            }
        }
    }
}
