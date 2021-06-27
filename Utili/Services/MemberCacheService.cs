using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Microsoft.Extensions.Logging;

namespace Utili.Services
{
    public class MemberCacheService
    {
        private readonly ILogger<MemberCacheService> _logger;
        private readonly DiscordClientBase _client;

        private List<ulong> _cachedGuilds;
        private Timer _timer;

        public MemberCacheService(ILogger<MemberCacheService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;

            _cachedGuilds = new();
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
                _logger.LogInformation("Caching users for {Guilds} guilds on {ShardId}", guildIds.Count, e.ShardId);
                await CacheMembersAsync(guildIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on ready");
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
            });
        }

        private async Task CacheMembersAsync(IEnumerable<ulong> guildIds)
        {
            guildIds = guildIds.Distinct();
            lock (_cachedGuilds)
            {
                foreach (var guildId in guildIds)
                {
                    _logger.LogDebug("Caching members for {Guild}", guildId);
                    _cachedGuilds.Add(guildId);
                    
                    _ = Task.Run(async () =>
                    {
                        await _client.Chunker.ChunkAsync(_client.GetGuild(guildId));
                    });
                }
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
