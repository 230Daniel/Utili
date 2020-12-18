using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using static UtiliSite.Main;

namespace UtiliSite
{
    public static class DiscordModule
    {
        // TODO: Make asyncronous

        private static DiscordRestClient _client;

        private static DiscordCache _cachedClients = new DiscordCache(600);
        private static DiscordCache _cachedGuildLists = new DiscordCache(7.5);
        private static DiscordCache _cachedGuildUsers = new DiscordCache(60);
        private static DiscordCache _cachedGuilds = new DiscordCache(120);
        private static DiscordCache _cachedTextChannels = new DiscordCache(15);
        private static DiscordCache _cachedVoiceChannels = new DiscordCache(15);

        public static void Initialise()
        {
            _client = new DiscordRestClient();
            _client.LoginAsync(TokenType.Bot, _config.DiscordToken).GetAwaiter().GetResult();
        }

        public static async Task<DiscordRestClient> GetClientAsync(ulong userId, string token = null)
        {
            DiscordRestClient client;

            try
            {
                if (_cachedClients.TryGet(userId, out object cacheResult))
                {
                    client = cacheResult as DiscordRestClient;

                    if (client.LoginState == LoginState.LoggedIn)
                    {
                        return client;
                    }

                    _cachedClients.Remove(userId);
                }

                client = new DiscordRestClient();
                await client.LoginAsync(TokenType.Bearer, token);

                _cachedClients.Add(userId, client);
            }
            catch
            {
                return null;
            }

            return client;
        }

        public static async Task<List<RestUserGuild>> GetManageableGuildsAsync(DiscordRestClient client)
        {
            List<RestUserGuild> guilds;

            if (_cachedGuildLists.TryGet(client.CurrentUser.Id, out object cacheResult))
            {
                guilds = cacheResult as List<RestUserGuild>;
                return guilds;
            }

            guilds = (await client.GetGuildSummariesAsync().FlattenAsync()).Where(x => x.Permissions.ManageGuild).ToList();
            _cachedGuildLists.Add(client.CurrentUser.Id, guilds);
            return guilds;
        }

        private static async Task<RestGuildUser> GetGuildUserAsync(ulong userId, ulong guildId)
        {
            if (_cachedGuildUsers.TryGet($"{guildId}/{userId}", out object guildUserObj))
            {
                if (guildUserObj == null)
                {
                    _cachedGuildUsers.Remove($"{guildId}/{userId}");
                }
                else
                {
                    return (RestGuildUser) guildUserObj;
                }
            }

            try
            {
                RestGuildUser guildUser = _client.GetGuildUserAsync(guildId, userId).GetAwaiter().GetResult();
                if(guildUser != null) _cachedGuildUsers.Add($"{guildId}/{userId}", guildUser);
                return guildUser;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<bool> IsGuildManageableAsync(ulong userId, ulong guildId)
        {
            RestGuildUser user = await GetGuildUserAsync(userId, guildId);
            return user.GuildPermissions.ManageGuild;
        }

        public static async Task<string> GetBotNicknameAsync(ulong guildId)
        {
            string nickname = (await GetGuildUserAsync(_client.CurrentUser.Id, guildId)).Nickname;
            return string.IsNullOrEmpty(nickname) ? _client.CurrentUser.Username : nickname;
        }

        public static async Task<RestGuild> GetGuildAsync(ulong guildId)
        {
            if (_cachedGuilds.TryGet(guildId, out object guildObj))
            {
                if (guildObj == null)
                {
                    _cachedGuilds.Remove(guildId);
                }
                else
                {
                    return (RestGuild) guildObj;
                }
            }

            try
            {
                RestGuild guild = await _client.GetGuildAsync(guildId);
                if(guild != null) _cachedGuilds.Add(guildId, guild);
                return guild;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<List<RestTextChannel>> GetTextChannelsAsync(RestGuild guild)
        {
            if (_cachedTextChannels.TryGet(guild.Id, out object channelsObj))
            {
                return (List<RestTextChannel>) channelsObj;
            }

            try
            {
                List<RestTextChannel> channels = (await guild.GetTextChannelsAsync()).ToList();
                _cachedTextChannels.Add(guild.Id, channels);
                return channels.OrderBy(x => x.Position).ToList();
            }
            catch
            {
                return null;
            }
        }

        public static async Task<List<RestVoiceChannel>> GetVoiceChannelsAsync(RestGuild guild)
        {
            if (_cachedVoiceChannels.TryGet(guild.Id, out object channelsObj))
            {
                return (List<RestVoiceChannel>) channelsObj;
            }

            try
            {
                List<RestVoiceChannel> channels = (await guild.GetVoiceChannelsAsync()).OrderBy(x => x.Position).ToList();
                _cachedVoiceChannels.Add(guild.Id, channels);
                return channels.OrderBy(x => x.Position).ToList();
            }
            catch
            {
                return null;
            }
        }

        public static async Task SetNicknameAsync(ulong guildId, string nickname)
        {
            RestGuildUser user = await GetGuildUserAsync(_client.CurrentUser.Id, guildId);
            await user.ModifyAsync(x => x.Nickname = nickname);
        }

        public static string GetGuildIconUrl(RestGuild guild)
        {
            string url = guild.IconUrl;
            if (string.IsNullOrEmpty(url)) return "https://cdn.discordapp.com/embed/avatars/0.png";

            url = url.Remove(url.Length - 4);
            url += ".png?size=256";
            return url;
        }

        public static string GetGuildIconUrl(RestUserGuild guild)
        {
            string url = guild.IconUrl;
            if (string.IsNullOrEmpty(url)) return "https://cdn.discordapp.com/embed/avatars/0.png";

            url = url.Remove(url.Length - 4);
            url += ".png?size=256";
            return url;
        }
    }

    internal class DiscordCache
    {
        public TimeSpan Timeout { get; }
        public List<DiscordCacheItem> Items { get; }

        public DiscordCache(double timeoutSeconds)
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds);
            Items = new List<DiscordCacheItem>();
        }

        public void Add(object key, object value)
        {
            DiscordCacheItem item = new DiscordCacheItem(key, value, Timeout);
            Items.RemoveAll(x => x.Key == key);
            Items.Add(item);

            Items.RemoveAll(x => x.Expiry < DateTime.Now);
        }

        public void Remove(object key)
        {
            Items.RemoveAll(x => x.Key == key);
        }

        public bool TryGet(object key, out object value)
        {
            value = null;
            try
            {
                Items.RemoveAll(x => x.Expiry < DateTime.Now);

                List<DiscordCacheItem> matches = Items.Where(x => x.Key == key).ToList();

                if (Items.Count == 0)
                {
                    return false;
                }

                if (Items.Count == 1)
                {
                    value = matches.First().Value;
                    return true;
                }

                // If by some extremely unlikely circimstance there are multiple matches,
                // Return the latest cached value
                matches = matches.OrderBy(x => x.Expiry).ToList();
                value = matches.Last().Value;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    internal class DiscordCacheItem
    {
        public object Key { get; }
        public DateTime Expiry { get; }
        public object Value { get; }

        public DiscordCacheItem(object key, object value, TimeSpan timeout)
        {
            Key = key;
            Expiry = DateTime.Now.Add(timeout);
            Value = value;
        }
    }
}
