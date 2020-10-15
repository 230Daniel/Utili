using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using static Utili.Program;

namespace Utili.Handlers
{
    internal class ReadyHandler
    {
        public static async Task ShardReady(DiscordSocketClient shard)
        {
            _ = Task.Run(async () =>
            {
                await shard.SetGameAsync("utili.bot | help");
            });
        }
    }
}
