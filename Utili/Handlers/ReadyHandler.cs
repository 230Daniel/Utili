using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

                Database.Sharding.UpdateShardStats(_client.Shards.Count,
                    _client.Shards.OrderBy(x => x.ShardId).First().ShardId, _client.Guilds.Count);

                _shardStatsUpdater = new Timer(10000);
                _shardStatsUpdater.Elapsed += Sharding.Update;
                _shardStatsUpdater.Start();
            });
        }
    }
}
