using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Database;
using Database.Entities;
using Database.Extensions;

namespace Utili.Services
{
    public class CoreConfigurationCacheService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(5);
        
        private static Dictionary<Snowflake, SemaphoreSlim> _semaphores = new();
        private static ConcurrentDictionary<Snowflake, CachedCoreConfiguration> _cachedConfigurations = new();
        
        private readonly DatabaseContext _dbContext;

        public CoreConfigurationCacheService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CoreConfiguration> GetCoreConfigurationAsync(Snowflake guildId)
        {
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
                var now = DateTime.UtcNow;
                if (_cachedConfigurations.TryGetValue(guildId, out var cachedConfiguration))
                {
                    if (cachedConfiguration.ExpiresAt >= now)
                    {
                        return cachedConfiguration.Configuration;
                    }
                    _cachedConfigurations.TryRemove(guildId, out _);
                }

                var config = await _dbContext.CoreConfigurations.GetForGuildAsync(guildId);
                _cachedConfigurations.TryAdd(guildId, new CachedCoreConfiguration(config, now + CacheDuration));
                
                return config;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private class CachedCoreConfiguration
        {
            public CoreConfiguration Configuration { get; }
            public DateTime ExpiresAt { get; }

            public CachedCoreConfiguration(CoreConfiguration configuration, DateTime expiresAt)
            {
                Configuration = configuration;
                ExpiresAt = expiresAt;
            }
        }
    }
}
