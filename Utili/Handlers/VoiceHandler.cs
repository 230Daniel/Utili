using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Handlers
{
    class VoiceHandler
    {
        public static async Task UserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            _ = VoiceLink(user, before, after);
            _ = InactiveRole(user, before, after);
        }

        public static async Task VoiceLink(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            _ = Task.Run(() =>
            {
                if(user.IsBot) return;

                if (Helper.RequiresUpdate(before, after))
                {
                    if(before.VoiceChannel != null) _voiceLink.RequestUpdate(before.VoiceChannel);
                    if(after.VoiceChannel != null) _voiceLink.RequestUpdate(after.VoiceChannel);

                    _voiceRoles.RequestUpdate(user as SocketGuildUser, before, after);
                }
            });
        }

        public static async Task InactiveRole(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            _ = Task.Run(async () =>
            {
                if(user.IsBot) return;

                if (Helper.RequiresUpdate(before, after))
                {
                    SocketGuildUser guildUser = user as SocketGuildUser;
                    await _inactiveRole.UpdateUserAsync(guildUser.Guild, guildUser);
                }
            });
        }
    }
}
