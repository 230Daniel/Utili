using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using NewDatabase;
using NewDatabase.Entities;
using NewDatabase.Extensions;

namespace Utili.Services
{
    public class CoreConfigurationCacheService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(5);
        
        private static Dictionary<Snowflake, SemaphoreSlim> _semaphores = new();
        private static Dictionary<Snowflake, (DateTime, CoreConfiguration)> _cachedConfigurations = new();
        
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
                    if (cachedConfiguration.Item1 > now)
                    {
                        return cachedConfiguration.Item2;
                    }
                    _cachedConfigurations.Remove(guildId);
                }
                
                var config = await _dbContext.CoreConfigurations.GetForGuildAsync(guildId);
                _cachedConfigurations.Add(config.GuildId, (now + CacheDuration, config));
                
                return config;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
