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
                }

                await DownloadRequiredUsersAsync(shard);

                await shard.SetGameAsync($"{_config.Domain} | .help");

                _readyShardIds.Add(shard.ShardId);
            });
        }

        private static async Task DownloadRequiredUsersAsync(DiscordSocketClient shard)
        {
            List<ulong> guildIds = new List<ulong>();
            List<SocketGuild> guilds = shard.Guilds.ToList();
            guilds = guilds.Where(x => x.DownloadedMemberCount < x.MemberCount).ToList();

            // Role persist enabled
            guildIds.AddRange(Database.Data.Roles.GetRows().Where(x => x.RolePersist).Select(x => x.GuildId));

            guilds = guilds.Where(x => guildIds.Contains(x.Id)).ToList();
            await shard.DownloadUsersAsync(guilds);

            _logger.Log($"Shard {shard.ShardId}", $"{guilds.Count(x => x.HasAllMembers)}/{guilds.Count} required guild user downloads completed", LogSeverity.Info);
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
