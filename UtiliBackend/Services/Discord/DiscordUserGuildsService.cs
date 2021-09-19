using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Http;

namespace UtiliBackend.Services
{
    public class DiscordUserGuildsService
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(15);
        
        private readonly DiscordClientService _discordClientService;
        private readonly Dictionary<ulong, (IEnumerable<RestUserGuild>, DateTime)> _cachedUserGuilds;
        
        public DiscordUserGuildsService(DiscordClientService discordClientService)
        {
            _discordClientService = discordClientService;
            _cachedUserGuilds = new();
        }

        public async Task<IEnumerable<RestUserGuild>> GetGuildsAsync(HttpContext httpContext)
        {
            var client = await _discordClientService.GetClientAsync(httpContext);
            if (client is null) return Array.Empty<RestUserGuild>();

            lock (_cachedUserGuilds)
            {
                var now = DateTime.UtcNow;
                foreach (var cachedValue in _cachedUserGuilds)
                {
                    if (cachedValue.Value.Item2 <= now)
                        _cachedUserGuilds.Remove(cachedValue.Key);
                }

                if (_cachedUserGuilds.TryGetValue(client.CurrentUser.Id, out var tuple))
                {
                    return tuple.Item1;
                }
            }

            var guilds = (await client.GetGuildSummariesAsync().FlattenAsync());
            
            lock (_cachedUserGuilds)
            {
                _cachedUserGuilds.Remove(client.CurrentUser.Id);
                _cachedUserGuilds.Add(client.CurrentUser.Id, (guilds, DateTime.UtcNow.Add(CacheDuration)));
            }

            return guilds;
        }
        
        public async Task<IEnumerable<RestUserGuild>> GetManagedGuildsAsync(HttpContext httpContext)
        {
            var guilds = await GetGuildsAsync(httpContext);
            return guilds.Where(x => x.Permissions.ManageGuild);
        }
    }
}
