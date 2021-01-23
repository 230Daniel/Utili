using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Discord;
using Discord.WebSocket;
using static Utili.Program;
using static Utili.MessageSender;

namespace Utili.Handlers
{
    internal static class ShardHandler
    {
        public static List<(int, DateTime)> ReadyShardIds = new List<(int, DateTime)>();
        
        public static async Task ShardReady(DiscordSocketClient shard)
        {
            _ = Task.Run(async () =>
            {
                // On rare occasions, Ready is fired before Connected,
                // causing no guilds to have their users downloaded.
                // The issue has been reported to the library maintainers.

                await Task.Delay(1000);
                while (shard.ConnectionState != ConnectionState.Connected) await Task.Delay(25);
                
                ReadyShardIds.RemoveAll(x => x.Item1 == shard.ShardId);
                if (_client.Shards.All(x => x.ConnectionState == ConnectionState.Connected)) _ = AllShardsReady();

                await DownloadRequiredUsersAsync(shard);
                await shard.SetGameAsync($"{_config.Domain} | {_config.DefaultPrefix}help");
                ReadyShardIds.Add((shard.ShardId, DateTime.Now));
            });
        }

        private static async Task AllShardsReady()
        {
            if (_config.Production)
            {
                Community.Initialise();

                await Database.Sharding.UpdateShardStatsAsync(_client.Shards.Count,
                    _client.Shards.OrderBy(x => x.ShardId).First().ShardId, _client.Guilds.Count);

                _shardStatsUpdater?.Dispose();
                _shardStatsUpdater = new Timer(10000);
                _shardStatsUpdater.Elapsed += Sharding.Update;
                _shardStatsUpdater.Start();
            }
                    
            _downloadNewRequiredUsersTimer?.Dispose();
            _downloadNewRequiredUsersTimer = new Timer(60000);
            _downloadNewRequiredUsersTimer.Elapsed += DownloadNewRequiredUsersTimer_Elapsed;
            _downloadNewRequiredUsersTimer.Start();
        }

        private static async Task DownloadRequiredUsersAsync(DiscordSocketClient shard)
        {
            List<ulong> guildIds = new List<ulong>();
            List<SocketGuild> guilds = shard.Guilds.ToList();
            guilds = guilds.Where(x => x.DownloadedMemberCount < x.MemberCount).ToList();

            // Role persist enabled
            guildIds.AddRange((await RolePersist.GetRowsAsync()).Where(x => x.Enabled).Select(x => x.GuildId));

            // Community server
            guildIds.Add(_config.Community.GuildId);

            guilds = guilds.Where(x => guildIds.Contains(x.Id)).ToList();
            await shard.DownloadUsersAsync(guilds);

            _logger.Log($"Shard {shard.ShardId}", $"{guilds.Count(x => x.HasAllMembers)}/{guilds.Count} required guild user downloads completed");
        }

        public static async Task Log(LogMessage logMessage)
        {
            string source = logMessage.Source.Replace("Shard #", "Shard ");

            if (logMessage.Exception == null)
            {
                _logger.Log(source, logMessage.Message, Helper.ConvertToLocalLogSeverity(logMessage.Severity));
            }
            else
            {
                if (logMessage.Exception.Message == "Server requested a reconnect")
                {
                    _logger.Log(source, "Server requested a reconnect", LogSeverity.Info);
                }
                else
                {
                    _logger.ReportError(source, logMessage.Exception, Helper.ConvertToLocalLogSeverity(logMessage.Severity));
                }
            }
        }

        public static async Task JoinedGuild(SocketGuild guild)
        {
            _ = Task.Run(async() =>
            {
                await SendInfoAsync(guild.DefaultChannel, "Hi, thanks for adding me!", 
                    $"Head to the [dashboard](https://{_config.Domain}/dashboard/{guild.Id}/core) to set up the features you want to use.\n" +
                    "If you require assistance, you can join the [Discord server](https://discord.gg/WsxqABZ) and we'll be happy to help you out.\n" +
                    "Have fun!");
            });
        }


        private static Timer _downloadNewRequiredUsersTimer;
        private static void DownloadNewRequiredUsersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = DownloadNewRequiredUsersAsync();
        }

        private static bool _downloadingNewRequiredUsers;
        private static async Task DownloadNewRequiredUsersAsync()
        {
            if(_downloadingNewRequiredUsers) return;
            _downloadingNewRequiredUsers = true;
            try
            {
                List<MiscRow> rows = await Misc.GetRowsAsync(null, "RequiresUserDownload");
                foreach(MiscRow row in rows)
                {
                    if(_client.Guilds.Any(x => x.Id == row.GuildId))
                    {
                        await Misc.DeleteRowAsync(row);
                        SocketGuild guild = _client.GetGuild(row.GuildId);
                        if (!guild.HasAllMembers) await guild.DownloadUsersAsync();
                        await Task.Delay(5000);
                    }
                }
            }
            catch { }
            _downloadingNewRequiredUsers = false;
        }
    }
}
