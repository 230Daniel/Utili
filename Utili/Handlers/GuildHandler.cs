using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Handlers
{
    internal class GuildHandler
    {
        public static async Task UserJoined(SocketGuildUser user)
        {
            _ = Task.Run(async () =>
            {
                _joinMessage.UserJoined(user);
            });
        }
    }
}
