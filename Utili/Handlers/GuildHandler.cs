using System.Threading.Tasks;
using Discord.WebSocket;
using Utili.Features;

namespace Utili.Handlers
{
    internal static class GuildHandler
    {
        public static async Task UserJoined(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                _ = JoinMessage.UserJoined(user);

                // Await these so only one role is added per second
                await RolePersist.UserJoined(user);
                await JoinRoles.UserJoined(user);
            });
        }

        public static async Task UserLeft(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                _ = RolePersist.UserLeft(user);
            });
        }
    }
}
