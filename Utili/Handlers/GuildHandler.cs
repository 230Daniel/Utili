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
                _ = Roles.UserJoined(user);
                _ = JoinMessage.UserJoined(user);
            });
        }

        public static async Task UserLeft(SocketGuildUser user)
        {
            _ = Roles.UserLeft(user);
        }
    }
}
