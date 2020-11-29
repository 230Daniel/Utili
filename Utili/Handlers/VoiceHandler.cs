using System.Threading.Tasks;
using Discord.WebSocket;
using Utili.Features;

namespace Utili.Handlers
{
    internal static class VoiceHandler
    {
        public static async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            _ = VoiceLinkHandler(user, before, after);
            _ = InactiveRoleHandler(user, before, after);
        }

        private static async Task VoiceLinkHandler(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            _ = Task.Run(() =>
            {
                if(user.IsBot) return;

                if (Helper.RequiresUpdate(before, after))
                {
                    if(before.VoiceChannel != null) VoiceLink.RequestUpdate(before.VoiceChannel);
                    if(after.VoiceChannel != null) VoiceLink.RequestUpdate(after.VoiceChannel);

                    VoiceRoles.RequestUpdate(user as SocketGuildUser, before, after);
                }
            });
        }

        private static async Task InactiveRoleHandler(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            _ = Task.Run(async () =>
            {
                if(user.IsBot) return;

                if (Helper.RequiresUpdate(before, after))
                {
                    SocketGuildUser guildUser = user as SocketGuildUser;
                    await InactiveRole.UpdateUserAsync(guildUser.Guild, guildUser);
                }
            });
        }
    }
}
