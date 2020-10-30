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
        }

        public static async Task VoiceLink(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            _ = Task.Run(() =>
            {
                if (Helper.RequiresUpdate(before, after))
                {
                    if(before.VoiceChannel != null) _voiceLink.RequestUpdate(before.VoiceChannel);
                    if(after.VoiceChannel != null) _voiceLink.RequestUpdate(after.VoiceChannel);

                    _voiceRoles.RequestUpdate(user as SocketGuildUser, before, after);
                }
            });
        }
    }
}
