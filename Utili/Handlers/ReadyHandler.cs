using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using static Utili.Program;

namespace Utili.Handlers
{
    internal class ReadyHandler
    {
        private static int _shardsStarted;

        public static async Task ShardReady(DiscordSocketClient shard)
        {
            _ = Task.Run(async () =>
            {
                await shard.SetGameAsync("utili.bot | .help");

                _shardsStarted += 1;

                if (_shardsStarted == _config.UpperShardId - _config.LowerShardId + 1)
                {
                    Database.Sharding.UpdateShardStats(_client.Shards.Count,
                        _client.Shards.OrderBy(x => x.ShardId).First().ShardId, _client.Guilds.Count);

                    _shardStatsUpdater = new Timer(10000);
                    _shardStatsUpdater.Elapsed += Sharding.Update;
                    _shardStatsUpdater.Start();

                    _shardsStarted = 0;
                }
            });
        }
    }
}
