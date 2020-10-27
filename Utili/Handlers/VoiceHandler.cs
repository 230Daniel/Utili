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
                bool requireUpdate = false;

                if (before.VoiceChannel == null && after.VoiceChannel == null)
                {
                    return;
                }

                if (before.VoiceChannel == null && after.VoiceChannel != null)
                {
                    requireUpdate = true;
                }

                else if (after.VoiceChannel == null && before.VoiceChannel != null)
                {
                    requireUpdate = true;
                }

                else if (after.VoiceChannel.Id != before.VoiceChannel.Id)
                {
                    requireUpdate = true;
                }

                if (requireUpdate)
                {
                    if(before.VoiceChannel != null) _voicelink.RequestUpdate(before.VoiceChannel);
                    if(after.VoiceChannel != null) _voicelink.RequestUpdate(after.VoiceChannel);
                }
            });
        }
    }
}
