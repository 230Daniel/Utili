using System.Collections.Generic;
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
        private static bool _firstAllConnect = true;
        public static async Task ShardConnected(DiscordSocketClient shard)
        {
            _ = Task.Run(async () =>
            {
                if (_firstAllConnect && _client.Shards.Count(x => x.ConnectionState == ConnectionState.Connected) == _client.Shards.Count)
                {
                    _firstAllConnect = false;

                    Database.Sharding.UpdateShardStats(_client.Shards.Count,
                        _client.Shards.OrderBy(x => x.ShardId).First().ShardId, _client.Guilds.Count);

                    _shardStatsUpdater = new Timer(10000);
                    _shardStatsUpdater.Elapsed += Sharding.Update;
                    _shardStatsUpdater.Start();

                    await DownloadRequiredUsersAsync();
                    await _client.SetGameAsync($"{_config.Domain} | .help");
                    StartDownloadTimer();
                }
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

        private static bool _currentlyDownloading;
        private static void DownloadTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(!_currentlyDownloading) _ = DownloadRequiredUsersAsync();
        }

        public static async Task DownloadRequiredUsersAsync()
        {
            while (_currentlyDownloading) await Task.Delay(5000);

            _currentlyDownloading = true;

            List<ulong> guildIds = new List<ulong>();

            // Role persist enabled
            guildIds.AddRange(Database.Data.Roles.GetRows().Where(x => x.RolePersist).Select(x => x.GuildId));

            try
            {
                foreach (DiscordSocketClient shard in _client.Shards)
                {
                    foreach(SocketGuild guild in shard.Guilds.Where(x => x.DownloadedMemberCount < x.MemberCount && guildIds.Contains(x.Id)))
                    {
                        // Ratelimit is 120 requests per minute, I believe that this is global over all shards

                        _ = guild.DownloadUsersAsync();
                        await Task.Delay(900);
                    }
                }
            }
            catch { }

            _currentlyDownloading = false;
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
