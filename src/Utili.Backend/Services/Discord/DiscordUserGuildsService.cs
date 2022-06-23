using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.OAuth2;
using Microsoft.AspNetCore.Http;
using Utili.Backend.Extensions;

namespace Utili.Backend.Services
{
    public class DiscordUserGuildsService
    {
        private readonly Dictionary<Snowflake, UserGuilds> _guilds;
        private readonly SemaphoreSlim _semaphore;

        public DiscordUserGuildsService()
        {
            _guilds = new();
            _semaphore = new(1, 1);
        }

        public async Task<UserGuilds> GetGuildsAsync(HttpContext httpContext)
        {
            var client = httpContext.GetDiscordClient();
            var userId = client.Authorization.User.Id;

            await _semaphore.WaitAsync();

            try
            {
                if (_guilds.TryGetValue(userId, out var cachedGuilds))
                {
                    if (cachedGuilds.ExpiresAt > DateTimeOffset.Now)
                        return cachedGuilds;
                    _guilds.Remove(userId, out _);
                }

                var newGuilds = await client.Client.FetchGuildsAsync();
                var guilds = new UserGuilds()
                {
                    Guilds = newGuilds.ToList(),
                    ExpiresAt = DateTimeOffset.Now.AddSeconds(15)
                };

                _guilds.TryAdd(userId, guilds);
                return guilds;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<UserGuilds> GetManagedGuildsAsync(HttpContext httpContext)
        {
            var guilds = await GetGuildsAsync(httpContext);
            if (guilds is null) return null;
            guilds.Guilds.RemoveAll(x => !x.Permissions.ManageGuild);
            return guilds;
        }

        public class UserGuilds
        {
            public List<IPartialGuild> Guilds { get; set; }
            public DateTimeOffset ExpiresAt { get; set; }
        }
    }
}
