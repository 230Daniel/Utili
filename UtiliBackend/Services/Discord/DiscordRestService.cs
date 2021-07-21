using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Microsoft.Extensions.Configuration;

namespace UtiliBackend.Services
{
    public class DiscordRestService
    {
        private readonly DiscordRestClient _client;
        
        private static readonly TimeSpan GuildCacheDuration = TimeSpan.FromSeconds(20);
        private readonly Dictionary<ulong, (RestGuild, DateTime)> _cachedGuilds;
        
        public DiscordRestService(IConfiguration configuration)
        {
            _client = new DiscordRestClient();
            _client.LoginAsync(TokenType.Bot, configuration["Discord:Token"]).GetAwaiter().GetResult();

            _cachedGuilds = new();
        }

        public async Task<RestGuild> GetGuildAsync(ulong guildId)
        {
            lock (_cachedGuilds)
            {
                var now = DateTime.UtcNow;
                foreach (var cachedValue in _cachedGuilds)
                {
                    if (cachedValue.Value.Item2 <= now)
                        _cachedGuilds.Remove(cachedValue.Key);
                }

                if (_cachedGuilds.TryGetValue(guildId, out var tuple))
                {
                    return tuple.Item1;
                }
            }

            RestGuild guild;
            try
            {
                guild = await _client.GetGuildAsync(guildId);
            }
            catch
            {
                guild = null;
            }

            lock (_cachedGuilds)
            {
                _cachedGuilds.Remove(guildId);
                _cachedGuilds.Add(guildId, (guild, DateTime.Now.Add(GuildCacheDuration)));
            }

            return guild;
        }
    }
}
