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

                // Await these so only one role is added per second
                await RolePersist.UserJoined(user);
            });
        }

        public static async Task UserUpdated(SocketGuildUser before, SocketGuildUser after)
        {
            // Note to self: This method only works with cached before users!!!

            _ = Task.Run(async () =>
            {
                if (before.IsPending.HasValue && after.IsPending.HasValue && before.IsPending.Value && !after.IsPending.Value)
                {
                    // Force bypass of all other delays, the user is no longer pending
                }

                await RoleLinking.GuildUserUpdated(before, after);
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
