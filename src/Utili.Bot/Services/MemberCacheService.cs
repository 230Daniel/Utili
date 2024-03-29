﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Disqord;
using Disqord.Gateway;
using Disqord.Gateway.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Utili.Bot.Extensions;
using Timer = System.Timers.Timer;

namespace Utili.Bot.Services;

public class MemberCacheService
{
    private static readonly TimeSpan TemporaryCacheLength = TimeSpan.FromMinutes(10);

    private readonly ILogger<MemberCacheService> _logger;
    private readonly IConfiguration _configuration;
    private readonly UtiliDiscordBot _bot;
    private readonly IServiceScopeFactory _scopeFactory;

    private List<Snowflake> _cachedGuilds;
    private ConcurrentDictionary<Snowflake, DateTime> _tempCachedGuilds;
    private Dictionary<Snowflake, SemaphoreSlim> _semaphores;
    private Timer _timer;

    public MemberCacheService(ILogger<MemberCacheService> logger, IConfiguration configuration, UtiliDiscordBot bot, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _bot = bot;
        _scopeFactory = scopeFactory;

        _cachedGuilds = new();
        _tempCachedGuilds = new();
        _semaphores = new();
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
            await PermanentlyCacheMembersAsync(guildIds);

            _logger.LogInformation("Finished caching members for {Shard}", e.ShardId);

            var domain = _configuration["Services:WebsiteDomain"];
            var defaultPrefix = _configuration["Discord:DefaultPrefix"];
            var activity = new LocalActivity($"{domain} | {defaultPrefix}help", ActivityType.Playing);
            var readyShard = (_bot.ApiClient as IGatewayApiClient).Shards[e.ShardId];
            await readyShard.SetPresenceAsync(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown on ready");
        }
    }

    public async Task TemporarilyCacheMembersAsync(Snowflake guildId)
    {
        lock (_cachedGuilds)
        {
            if (_cachedGuilds.Contains(guildId))
                // The guild is cached permanently
                return;
        }

        SemaphoreSlim semaphore;

        lock (_semaphores)
        {
            if (!_semaphores.TryGetValue(guildId, out semaphore))
            {
                semaphore = new SemaphoreSlim(1, 1);
                _semaphores.Add(guildId, semaphore);
            }
        }

        await semaphore.WaitAsync();

        try
        {
            if (_tempCachedGuilds.TryGetValue(guildId, out var expiryTime) && expiryTime > DateTime.UtcNow)
            {
                // The expiry time is in the future, renew the expiry time
                _tempCachedGuilds[guildId] = DateTime.UtcNow.Add(TemporaryCacheLength);
                return;
            }

            // The expiry time is in the past, chunk members now and set expiry time
            await _bot.Chunker.ChunkAsync(_bot.GetGuild(guildId));
            _tempCachedGuilds[guildId] = DateTime.UtcNow.Add(TemporaryCacheLength);
            _logger.LogInformation("Temporarily cached members for {Guild}", guildId);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        _ = Task.Run(async () =>
        {
            var guildIdsToCheck = _bot.GetGuilds().Select(x => x.Key).ToList();
            guildIdsToCheck.RemoveAll(x => _cachedGuilds.Contains(x));

            var guildIds = await GetRequiredDownloadsAsync(guildIdsToCheck);
            await PermanentlyCacheMembersAsync(guildIds);
            await UncacheExpiredTemporaryMembersAsync();
        });
    }

    private async Task PermanentlyCacheMembersAsync(IEnumerable<Snowflake> guildIds)
    {
        try
        {
            lock (_cachedGuilds)
            {
                _cachedGuilds.AddRange(guildIds);
            }

            foreach (var guildId in guildIds)
            {
                var guild = _bot.GetGuild(guildId);
                if (guild is null) continue;
                await _bot.Chunker.ChunkAsync(guild);
                _logger.LogDebug("Cached members for {Guild}", guildId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while permanently caching members");
            throw;
        }
    }

    private async Task<List<Snowflake>> GetRequiredDownloadsAsync(IEnumerable<Snowflake> shardGuildIds)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.GetDbContext();
        var guildIds = new List<ulong>();

        var rolePersistConfigs = await db.RolePersistConfigurations.Where(x => x.Enabled).ToListAsync();
        guildIds.AddRange(rolePersistConfigs.Select(x => x.GuildId));

        var roleLinkingConfigs = await db.RoleLinkingConfigurations.ToListAsync();
        guildIds.AddRange(roleLinkingConfigs.Select(x => x.GuildId));

        guildIds.RemoveAll(x => !shardGuildIds.Contains(x));
        return guildIds.Distinct().Select(x => new Snowflake(x)).ToList();
    }

    private async Task UncacheExpiredTemporaryMembersAsync()
    {
        try
        {
            var guildIds = _tempCachedGuilds.Keys.ToArray();
            foreach (var guildId in guildIds)
            {
                lock (_cachedGuilds)
                {
                    if (_cachedGuilds.Contains(guildId))
                        // The guild is cached permanently
                        continue;
                }

                SemaphoreSlim semaphore;

                lock (_semaphores)
                {
                    if (!_semaphores.TryGetValue(guildId, out semaphore))
                    {
                        semaphore = new SemaphoreSlim(1, 1);
                        _semaphores.Add(guildId, semaphore);
                    }
                }

                await semaphore.WaitAsync();

                try
                {
                    if (_tempCachedGuilds.TryGetValue(guildId, out var expiryTime) && expiryTime < DateTime.UtcNow)
                    {
                        // The expiry time is in the past, remove it and un-cache
                        _tempCachedGuilds.TryRemove(guildId, out _);

                        if (!_bot.CacheProvider.TryGetMembers(guildId, out var cache))
                            continue;

                        var toRemove = cache.Where(x =>
                            {
                                var (memberId, member) = x;

                                if (memberId == _bot.CurrentUser.Id)
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
                finally
                {
                    semaphore.Release();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while uncaching expired temporary members");
        }
    }
}
