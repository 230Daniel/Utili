using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;

namespace Utili.Backend.Services
{
    public class DiscordRestService
    {
        private readonly IRestClient _client;

        private static readonly TimeSpan GuildCacheDuration = TimeSpan.FromSeconds(20);
        private readonly Dictionary<ulong, (IGuild, DateTime)> _cachedGuilds;

        private static readonly TimeSpan ChannelCacheDuration = TimeSpan.FromSeconds(20);
        private readonly Dictionary<ulong, (IEnumerable<IGuildChannel>, DateTime)> _cachedChannels;

        public DiscordRestService(IRestClient restClient)
        {
            _client = restClient;

            _cachedGuilds = new();
            _cachedChannels = new();
        }

        public async Task<IGuild> GetGuildAsync(ulong guildId)
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

            IGuild guild;
            try
            {
                guild = await _client.FetchGuildAsync(guildId);
            }
            catch
            {
                return null;
            }

            lock (_cachedGuilds)
            {
                _cachedGuilds.Remove(guildId);
                _cachedGuilds.Add(guildId, (guild, DateTime.UtcNow.Add(GuildCacheDuration)));
            }

            return guild;
        }

        public async Task<IEnumerable<IGuildChannel>> GetChannelsAsync(ulong guildId)
        {
            lock (_cachedChannels)
            {
                var now = DateTime.UtcNow;
                foreach (var cachedValue in _cachedChannels)
                {
                    if (cachedValue.Value.Item2 <= now)
                        _cachedChannels.Remove(cachedValue.Key);
                }

                if (_cachedChannels.TryGetValue(guildId, out var tuple))
                {
                    return tuple.Item1;
                }
            }

            var channels = await _client.FetchChannelsAsync(guildId);

            lock (_cachedChannels)
            {
                _cachedChannels.Remove(guildId);
                _cachedChannels.Add(guildId, (channels, DateTime.UtcNow.Add(ChannelCacheDuration)));
            }

            return channels;
        }

        public async Task<IEnumerable<ITextChannel>> GetTextChannelsAsync(ulong guildId)
        {
            return (await GetChannelsAsync(guildId)).OfType<ITextChannel>();
        }

        public async Task<IEnumerable<IVocalGuildChannel>> GetVocalChannelsAsync(ulong guildId)
        {
            return (await GetChannelsAsync(guildId)).OfType<IVocalGuildChannel>();
        }
    }
}
