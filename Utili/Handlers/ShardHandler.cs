using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Handlers
{
    internal class ShardHandler
    {
        private static int _shardsStarted;

        public static async Task ShardReady(DiscordSocketClient shard)
        {
            _ = Task.Run(async () =>
            {
                
            });
        }

        public static async Task ShardConnected(DiscordSocketClient shard)
        {
            _ = Task.Run(async () =>
            {
                if (_config.FillUserCache)
                {
                    foreach (SocketGuild guild in _client.Guilds)
                    {
                        await guild.DownloadUsersAsync();
                    }

                    int notDownloaded = _client.Guilds.Count(x => x.Users.Count != x.MemberCount);
                    if (notDownloaded > 0)
                    {
                        _logger.Log("Connected", $"Users not fully downloaded for {notDownloaded} guilds",
                            LogSeverity.Warn);
                    }
                }

                await shard.SetGameAsync($"{_config.Domain} | .help");

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

        public static async Task Log(LogMessage logMessage)
        {
            if (logMessage.Exception == null)
            {
                _logger.Log(logMessage.Source, logMessage.Message, Helper.ConvertToLocalLogSeverity(logMessage.Severity));
            }
            else
            {
                _logger.ReportError(logMessage.Source, logMessage.Exception);
            }
        }
    }
}
