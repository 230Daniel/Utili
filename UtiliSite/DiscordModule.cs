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

        public static void Initialise()
        {
            _client = new DiscordRestClient();
            _client.LoginAsync(TokenType.Bot, _config.DiscordToken).GetAwaiter().GetResult();
        }


        private static DiscordCache _cachedClients = new DiscordCache(600);
        public static DiscordRestClient GetClient(ulong userId, string token = null)
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
                client.LoginAsync(TokenType.Bearer, token).GetAwaiter().GetResult();

                _cachedClients.Add(userId, client);
            }
            catch
            {
                return null;
            }

            return client;
        }


        private static DiscordCache _cachedGuildLists = new DiscordCache(7.5);
        public static List<RestUserGuild> GetManageableGuilds(DiscordRestClient client)
        {
            List<RestUserGuild> guilds;

            if (_cachedGuildLists.TryGet(client.CurrentUser.Id, out object cacheResult))
            {
                guilds = cacheResult as List<RestUserGuild>;
                return guilds;
            }

            guilds = client.GetGuildSummariesAsync().FlattenAsync().GetAwaiter().GetResult().Where(x => x.Permissions.ManageGuild).ToList();
            _cachedGuildLists.Add(client.CurrentUser.Id, guilds);
            return guilds;
        }


        private static DiscordCache _cachedGuildUsers = new DiscordCache(60);
        public static RestGuildUser GetGuildUser(ulong userId, ulong guildId)
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

        public static bool IsGuildManageable(ulong userId, ulong guildId)
        {
            RestGuildUser user = GetGuildUser(userId, guildId);
            return user.GuildPermissions.ManageGuild;
        }

        private static DiscordCache _cachedGuilds = new DiscordCache(60);
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


        private static DiscordCache _cachedChannelNames = new DiscordCache(30);
        public static async Task<string> GetChannelNameAsync(RestGuild guild, ulong channelId)
        {
            if (_cachedChannelNames.TryGet(channelId, out object channelNameObj))
            {
                if (channelNameObj == null)
                {
                    _cachedChannelNames.Remove(channelId);
                }
                else
                {
                    return (string) channelNameObj;
                }
            }

            try
            {
                RestTextChannel channel = await guild.GetTextChannelAsync(channelId);
                _cachedChannelNames.Add(channelId, channel.Name);
                return channel.Name;
            }
            catch
            {
                return null;
            }
        }


        private static DiscordCache _cachedTextChannels = new DiscordCache(15);
        public static async Task<List<RestTextChannel>> GetTextChannelsAsync(RestGuild guild)
        {
            if (_cachedTextChannels.TryGet(guild.Id, out object channelsObj))
            {
                return (List<RestTextChannel>) channelsObj;
            }

            try
            {
                List<RestTextChannel> channels = (await guild.GetTextChannelsAsync()).OrderBy(x => x.Position).ToList();
                _cachedTextChannels.Add(guild.Id, channels);
                return channels;
            }
            catch
            {
                return null;
            }
        }

        private static DiscordCache _cachedVoiceChannels = new DiscordCache(15);
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
                return channels;
            }
            catch
            {
                return null;
            }
        }

        public static string GetNickname(RestGuild guild)
        {
            string nickname = guild.GetUserAsync(_client.CurrentUser.Id).GetAwaiter().GetResult().Nickname;

            if (string.IsNullOrEmpty(nickname))
            {
                nickname = _client.CurrentUser.Username;
            }

            return nickname;
        }
        public static void SetNickname(RestGuild guild, string nickname)
        {
            _ = guild.GetUserAsync(_client.CurrentUser.Id).GetAwaiter().GetResult()
                .ModifyAsync(x => x.Nickname = nickname);
        }


        public static string SanitiseIconUrl(string url)
        {
            if (url == null)
            {
                url = "https://cdn.discordapp.com/attachments/591310067979255808/763519555447291915/GreySquare.png";
            }

            return url;
        }

        public static string GetGuildIconUrl(RestGuild guild)
        {
            string url = guild.IconUrl;
            if (string.IsNullOrEmpty(url)) return "https://cdn.discordapp.com/embed/avatars/0.png";

            url = url.Remove(url.Length - 4);
            url += ".png?size=1024";
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
