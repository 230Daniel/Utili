using System.Threading.Tasks;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Handlers
{
    internal static class GuildHandler
    {
        public static async Task UserJoined(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                _ = _roles.UserJoined(user);
                _ = _joinMessage.UserJoined(user);
            });
        }

        public static async Task UserLeft(SocketGuildUser user)
        {
            _ = _roles.UserLeft(user);
        }
    }
}
