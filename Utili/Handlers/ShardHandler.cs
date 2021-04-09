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
        public static List<(int, DateTime)> ShardRegister { get; } = new List<(int, DateTime)>();
        private static bool _allShardsReadyFired;
        public static async Task ShardReady(DiscordSocketClient shard)
        {
            _ = Task.Run(async () =>
            {
                // On rare occasions, Ready is fired before Connected,
                // causing no guilds to have their users downloaded.
                // The issue has been reported to the library maintainers.

                await Task.Delay(1000);
                while (shard.ConnectionState != ConnectionState.Connected) await Task.Delay(25);
                
                ShardRegister.RemoveAll(x => x.Item1 == shard.ShardId);
                ShardRegister.Add((shard.ShardId, DateTime.Now));

                if (_oldClient.Shards.All(x => ShardRegister.Any(y => y.Item1 == x.ShardId))) _ = AllShardsReady();

                await CacheUsersAsync(shard);
                await shard.SetGameAsync($"{_config.Domain} | {_config.DefaultPrefix}help");
            });
        }

        private static async Task AllShardsReady()
        {
            if (_allShardsReadyFired) return;
            _allShardsReadyFired = true;

            if (_config.Production)
            {
                Community.Initialise();

                await Database.Sharding.UpdateShardStatsAsync(_oldClient.Shards.Count,
                    _oldClient.Shards.OrderBy(x => x.ShardId).First().ShardId, _oldClient.Guilds.Count);

                _shardStatsUpdater?.Dispose();
                _shardStatsUpdater = new Timer(10000);
                _shardStatsUpdater.Elapsed += Sharding.Update;
                _shardStatsUpdater.Start();

                Monitoring.Start();
            }
                    
            _userCacheTimer?.Dispose();
            _userCacheTimer = new Timer(30000);
            _userCacheTimer.Elapsed += UserCacheTimerElapsed;
            _userCacheTimer.Start();

            Features.Autopurge.Start();
        }

        private static async Task CacheUsersAsync(DiscordSocketClient shard)
        {
            List<ulong> userCacheGuildId = await GetUserCacheGuildsAsync();
            List<SocketGuild> guilds = shard.Guilds.Where(x => userCacheGuildId.Contains(x.Id)).ToList();
            await shard.DownloadUsersAsync(guilds);

            _logger.Log($"Shard {shard.ShardId}", $"{guilds.Count(x => x.HasAllMembers)}/{guilds.Count} required guild user downloads completed");
        }

        private static async Task<List<ulong>> GetUserCacheGuildsAsync()
        {
            List<ulong> guildIds = new List<ulong>();

            // Role persist enabled
            guildIds.AddRange((await RolePersist.GetRowsAsync()).Where(x => x.Enabled).Select(x => x.GuildId));

            // Role linking
            guildIds.AddRange((await RoleLinking.GetRowsAsync()).Select(x => x.GuildId));

            // Community server
            guildIds.Add(_config.Community.GuildId);

            // Servers that have used commands like random
            lock (_guaranteedDownloads)
            {
                _guaranteedDownloads.RemoveAll(x => x.Item2 < DateTime.Now);
                guildIds.AddRange(_guaranteedDownloads.Select(x => x.Item1));
            }

            return guildIds.Distinct().ToList();
        }

        public static async Task Log(LogMessage logMessage)
        {
            string source = logMessage.Source.Replace("Shard #", "Shard ");

            if (logMessage.Exception is null)
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
                await SendInfoAsync(guild.DefaultChannel, "Utili v2", 
                    "Hi, thanks for adding me!\n" +
                    $"Head to the [dashboard](https://{_config.Domain}/dashboard/{guild.Id}/core) to configure the bot.\n" +
                    "If you need help or have any questions, you can join the [Discord server](https://discord.gg/WsxqABZ)");
            });
        }

        public static async Task LeftGuild(SocketGuild guild)
        {
            _ = Task.Run(async() =>
            {
                DateTimeOffset? joinedAt = guild.GetUser(_oldClient.CurrentUser.Id).JoinedAt;
                if (joinedAt.HasValue)
                {
                    TimeSpan stay = DateTime.UtcNow - joinedAt.Value.UtcDateTime;
                    _logger.Log("Left", $"Left {guild.Name} ({guild.Id}) - Stayed for {stay.ToLongString()}");
                }
                else
                {
                    _logger.Log("Left", $"Left {guild.Name} ({guild.Id}) - Stayed for unknown");
                }
            });
        }


        private static Timer _userCacheTimer;
        private static bool _updatingUserCache;
        private static List<(ulong, DateTime)> _guaranteedDownloads = new List<(ulong, DateTime)>();

        private static void UserCacheTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _ = UpdateCacheAsync();
        }

        private static async Task UpdateCacheAsync()
        {
            if(_updatingUserCache) return;
            _updatingUserCache = true;

            try
            {
                // Download guild users which now require caching

                List<MiscRow> rows = await Misc.GetRowsAsync(null, "RequiresUserDownload");
                foreach (MiscRow row in rows)
                {
                    if (_oldClient.Guilds.Any(x => x.Id == row.GuildId))
                    {
                        await Misc.DeleteRowAsync(row);
                        SocketGuild guild = _oldClient.GetGuild(row.GuildId);
                        if (!guild.HasAllMembers) await guild.DownloadUsersAsync();
                        await Task.Delay(5000);
                    }
                }

                // Clear unnecessary downloads to free up memory

                //lock (_guaranteedDownloads)
                //{
                    //List<ulong> userCacheGuildIds = GetUserCacheGuildsAsync().GetAwaiter().GetResult();
                    //foreach (SocketGuild guild in _client.Guilds.Where(x => !userCacheGuildIds.Contains(x.Id)))
                    //{
                    //    guild.ClearUserCache(x => x.VoiceChannel is null && (x.IsPending is null || !x.IsPending.Value));
                    //}
                //}
            }
            catch (Exception e)
            {
                _logger.ReportError("Cache", e);
            }

            _updatingUserCache = false;
        }

        public static async Task DownloadAndKeepUsersAsync(this SocketGuild guild, TimeSpan? keepFor = null)
        {
            // Guarantees that the user cache will be downloaded for that server until keepFor is up.

            DateTime keepUntil = DateTime.MaxValue;
            if(keepFor.HasValue) keepUntil = DateTime.UtcNow + keepFor.Value;

            lock (_guaranteedDownloads)
            {
                if (_guaranteedDownloads.Any(x => x.Item1 == guild.Id && x.Item2 > DateTime.UtcNow))
                {
                    if (_guaranteedDownloads.First(x => x.Item1 == guild.Id).Item2 < keepUntil)
                    {
                        _guaranteedDownloads.RemoveAll(x => x.Item1 == guild.Id);
                        _guaranteedDownloads.Add((guild.Id, keepUntil));
                    }
                    return;
                }

                _guaranteedDownloads.RemoveAll(x => x.Item1 == guild.Id);
                _guaranteedDownloads.Add((guild.Id, keepUntil));
            }
            await guild.DownloadUsersAsync();
        }
    }
}
