using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Handlers
{
    internal static class ShardHandler
    {
        private static List<int> _readyShardIds = new List<int>();

        public static async Task ShardConnected(DiscordSocketClient shard)
        {
            _ = Task.Run(async () =>
            {
                _readyShardIds.RemoveAll(x => x == shard.ShardId);

                if (_client.Shards.All(x => x.ConnectionState == ConnectionState.Connected))
                {
                    Database.Sharding.UpdateShardStats(_client.Shards.Count,
                        _client.Shards.OrderBy(x => x.ShardId).First().ShardId, _client.Guilds.Count);

                    _shardStatsUpdater?.Dispose();
                    _shardStatsUpdater = new Timer(10000);
                    _shardStatsUpdater.Elapsed += Sharding.Update;
                    _shardStatsUpdater.Start();

                    StartDownloadTimer();
                }

                await DownloadRequiredUsersAsync(shard); 
                // TODO: Ensure all relevant guilds are downloaded before continuing

                await shard.SetGameAsync($"{_config.Domain} | .help");

                _readyShardIds.Add(shard.ShardId);
            });
        }

        private static Timer _downloadTimer;
        private static void StartDownloadTimer()
        {
            _downloadTimer?.Dispose();

            _downloadTimer = new Timer(60000);
            _downloadTimer.Elapsed += DownloadTimer_Elapsed;
            _downloadTimer.Start();
        }

        private static void DownloadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = DownloadRequiredUsersAsync();
        }

        public static async Task DownloadRequiredUsersAsync(DiscordSocketClient specificShard = null)
        {
            try
            {
                List<ulong> guildIds = new List<ulong>();
                List<SocketGuild> guilds = new List<SocketGuild>();
                if (specificShard != null) guilds = specificShard.Guilds.ToList();
                else
                {
                    foreach (DiscordSocketClient shard in _client.Shards.Where(x => _readyShardIds.Contains(x.ShardId)))
                    {
                        guilds.AddRange(shard.Guilds);
                    }
                }
                guilds = guilds.Where(x => x.DownloadedMemberCount < x.MemberCount).ToList();

                // Role persist enabled
                guildIds.AddRange(Database.Data.Roles.GetRows().Where(x => x.RolePersist).Select(x => x.GuildId));

                guilds = guilds.Where(x => guildIds.Contains(x.Id)).ToList();
                await _client.DownloadUsersAsync(guilds);
            }
            catch { }
        }

        public static async Task Log(LogMessage logMessage)
        {
            if (logMessage.Exception == null)
            {
                _logger.Log(logMessage.Source, logMessage.Message, Helper.ConvertToLocalLogSeverity(logMessage.Severity));
            }
            else
            {
                if (logMessage.Exception.Message == "Server requested a reconnect")
                {
                    _logger.Log(logMessage.Source, "Server requested a reconnect", LogSeverity.Info);
                }
                else
                {
                    _logger.ReportError(logMessage.Source, logMessage.Exception, Helper.ConvertToLocalLogSeverity(logMessage.Severity));
                }
            }
        }
    }
}
