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
        private readonly IConfiguration _configuration;
        private readonly DiscordRestClient _client;
        
        private static readonly TimeSpan GuildCacheDuration = TimeSpan.FromSeconds(20);
        private readonly Dictionary<ulong, (RestGuild, DateTime)> _cachedGuilds;
        
        private static readonly TimeSpan TextChannelCacheDuration = TimeSpan.FromSeconds(20);
        private readonly Dictionary<ulong, (IEnumerable<RestTextChannel>, DateTime)> _cachedTextChannels;
        
        private static readonly TimeSpan VoiceChannelCacheDuration = TimeSpan.FromSeconds(20);
        private readonly Dictionary<ulong, (IEnumerable<RestVoiceChannel>, DateTime)> _cachedVoiceChannels;

        public DiscordRestService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            _client = new();
            _cachedGuilds = new();
            _cachedTextChannels = new();
            _cachedVoiceChannels = new();
        }

        public async Task InitialiseAsync()
        {
            await _client.LoginAsync(TokenType.Bot, _configuration["Discord:Token"]);
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
        
        public async Task<IEnumerable<RestTextChannel>> GetTextChannelsAsync(RestGuild guild)
        {
            lock (_cachedTextChannels)
            {
                var now = DateTime.UtcNow;
                foreach (var cachedValue in _cachedTextChannels)
                {
                    if (cachedValue.Value.Item2 <= now)
                        _cachedTextChannels.Remove(cachedValue.Key);
                }

                if (_cachedTextChannels.TryGetValue(guild.Id, out var tuple))
                {
                    return tuple.Item1;
                }
            }

            var channels = await guild.GetTextChannelsAsync();

            lock (_cachedTextChannels)
            {
                _cachedTextChannels.Remove(guild.Id);
                _cachedTextChannels.Add(guild.Id, (channels, DateTime.Now.Add(TextChannelCacheDuration)));
            }

            return channels;
        }
        
        public async Task<IEnumerable<RestVoiceChannel>> GetVoiceChannelsAsync(RestGuild guild)
        {
            lock (_cachedVoiceChannels)
            {
                var now = DateTime.UtcNow;
                foreach (var cachedValue in _cachedVoiceChannels)
                {
                    if (cachedValue.Value.Item2 <= now)
                        _cachedVoiceChannels.Remove(cachedValue.Key);
                }

                if (_cachedVoiceChannels.TryGetValue(guild.Id, out var tuple))
                {
                    return tuple.Item1;
                }
            }

            var channels = await guild.GetVoiceChannelsAsync();

            lock (_cachedVoiceChannels)
            {
                _cachedVoiceChannels.Remove(guild.Id);
                _cachedVoiceChannels.Add(guild.Id, (channels, DateTime.Now.Add(VoiceChannelCacheDuration)));
            }

            return channels;
        }
    }
}
