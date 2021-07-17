using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Utili.Services
{
    public class MemberCacheService
    {
        private static readonly TimeSpan TemporaryCacheLength = TimeSpan.FromMinutes(10);
        
        private readonly ILogger<MemberCacheService> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordClientBase _client;

        private List<ulong> _cachedGuilds;
        private Dictionary<ulong, DateTime> _tempCachedGuilds;
        private Dictionary<ulong, ValueTask<bool>> _tempCachedGuildTasks;
        private Timer _timer;

        public MemberCacheService(ILogger<MemberCacheService> logger, IConfiguration configuration, DiscordClientBase client)
        {
            _logger = logger;
            _configuration = configuration;
            _client = client;

            _cachedGuilds = new();
            _tempCachedGuilds = new();
            _tempCachedGuildTasks = new();
            _timer = new(10000);
            _timer.Elapsed += TimerElapsed;
        }

        public void Start()
        {
            _timer.Start();
        }

        public async Task Ready(ReadyEventArgs e)
        {
            try
            {
                var guildIds = await GetRequiredDownloadsAsync(e.GuildIds);
                _logger.LogInformation("Caching members for {Guilds} guilds on {ShardId}", guildIds.Count, e.ShardId);
                await CacheMembersAsync(guildIds);
                _logger.LogInformation("Finished caching members for {Shard}", e.ShardId);
                await e.CurrentUser.GetGatewayClient().SetPresenceAsync(new LocalActivity($"{_configuration.GetValue<string>("Domain")} | {_configuration.GetValue<string>("DefaultPrefix")}help", ActivityType.Playing));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on ready");
            }
        }

        public async Task TemporarilyCacheMembersAsync(ulong guildId)
        {
            lock (_cachedGuilds)
            {
                if (_cachedGuilds.Contains(guildId)) return;
            }
            
            ValueTask<bool> task = default;
            var alreadyCached = false;
            
            lock (_tempCachedGuilds)
            {
                if (_tempCachedGuilds.TryGetValue(guildId, out var expiryTime))
                {
                    if (expiryTime > DateTime.UtcNow)
                    {
                        alreadyCached = true;
                        lock (_tempCachedGuildTasks)
                        {
                            _tempCachedGuildTasks.TryGetValue(guildId, out task);
                        }
                    }
                }
            
                _tempCachedGuilds[guildId] = DateTime.UtcNow.Add(TemporaryCacheLength);
            }

            if (alreadyCached)
            {
                await task;
                return;
            }
            
            var newTask = _client.Chunker.ChunkAsync(_client.GetGuild(guildId));
            
            lock (_tempCachedGuildTasks)
            {
                _tempCachedGuildTasks[guildId] = newTask;
            }
            
            await newTask;
            
            lock (_tempCachedGuildTasks)
            {
                _tempCachedGuildTasks.Remove(guildId);
            }
            
            _logger.LogInformation("Temporarily cached members for {Guild}", guildId);
        }

        private void UncacheTemporaryMembersAsync()
        {
            lock (_tempCachedGuilds)
            {
                foreach (var tempCachedGuild in _tempCachedGuilds)
                {
                    var (guildId, expiry) = tempCachedGuild;
                
                    if (expiry < DateTimeOffset.UtcNow)
                    {
                        _tempCachedGuilds.Remove(guildId);
                    
                        lock (_cachedGuilds)
                        {
                            if (_cachedGuilds.Contains(guildId))
                                continue;
                        }

                        if(!_client.CacheProvider.TryGetMembers(guildId, out var cache))
                            continue;

                        var toRemove = cache.Where(x =>
                            {
                                var (userId, member) = x;
                            
                                if (userId == _client.CurrentUser.Id)
                                    return false;

                                if (member.IsPending)
                                    return false;

                                var voiceState = member.GetVoiceState();
                                return voiceState?.ChannelId is null;
                            })
                            .Select(x => x.Key);

                        foreach (var userId in toRemove)
                        {
                            cache.TryRemove(userId, out _);
                        }

                        _logger.LogInformation("Uncached members for guild {GuildId}", guildId);
                    }
                }
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var guildIdsToCheck = _client.GetGuilds().Select(x => x.Key).ToList();
                guildIdsToCheck.RemoveAll(x => _cachedGuilds.Contains(x));

                var guildIds = await GetRequiredDownloadsAsync(guildIdsToCheck);
                await CacheMembersAsync(guildIds);
                UncacheTemporaryMembersAsync();
            });
        }

        private async Task CacheMembersAsync(IEnumerable<ulong> guildIds)
        {
            guildIds = guildIds.Distinct();
            lock (_cachedGuilds)
            {
                _cachedGuilds.AddRange(guildIds);
            }
            
            foreach (var guildId in guildIds)
            {
                await _client.Chunker.ChunkAsync(_client.GetGuild(guildId));
                _logger.LogDebug("Cached members for {Guild}", guildId);
            }
        }

        private static async Task<List<ulong>> GetRequiredDownloadsAsync(IEnumerable<Snowflake> shardGuildIds)
        {
            var guildIds = new List<ulong>();
            guildIds.AddRange((await RolePersist.GetRowsAsync()).Where(x => x.Enabled).Select(x => x.GuildId));
            guildIds.AddRange((await RoleLinking.GetRowsAsync()).Select(x => x.GuildId));
            guildIds.RemoveAll(x => !shardGuildIds.Contains(x));
            return guildIds.Distinct().ToList();
        }
    }
}
