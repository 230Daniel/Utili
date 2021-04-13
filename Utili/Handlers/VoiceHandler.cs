using System.Threading.Tasks;
using Discord.WebSocket;
using Utili.Features;

namespace Utili.Handlers
{
    internal static class VoiceHandler
    {
        public static async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            _ = InactiveRoleHandler(user, before, after);
        }

        private static async Task InactiveRoleHandler(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            _ = Task.Run(async () =>
            {
                if(user.IsBot) return;

                if (false/*Helper.RequiresUpdate(before, after)*/)
                {
                    SocketGuildUser guildUser = user as SocketGuildUser;
                    await InactiveRole.UpdateUserAsync(guildUser.Guild, guildUser);
                }
            });
        }
    }
}
