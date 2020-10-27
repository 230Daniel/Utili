using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Net.Rest;
using Discord;
using Microsoft.AspNetCore.Http;
using AspNet.Security.OAuth.Discord;
using static UtiliSite.Main;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Discord.Rest;
using Discord.WebSocket;
using System.Timers;

namespace UtiliSite
{
    public class DiscordModule
    {
        public static DiscordRestClient _client;

        public static void Initialise()
        {
            _client = new DiscordRestClient();
            _client.LoginAsync(TokenType.Bot, _config.DiscordToken).GetAwaiter().GetResult();
        }


        private static DiscordCache _cachedClients = new DiscordCache(900);
        public static DiscordRestClient GetClient(ulong userId, string token = null)
        {
            DiscordRestClient client;

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
        public static bool IsGuildManageable(DiscordRestClient client, ulong guildId)
        {
            List<RestUserGuild> guilds = GetManageableGuilds(client);
            return guilds.Select(x => x.Id).Contains(guildId);
        }


        private static DiscordCache _cachedGuilds = new DiscordCache(7.5);
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
                List<RestTextChannel> channels = (await guild.GetTextChannelsAsync()).OrderBy(x => x.Name).ToList();
                _cachedTextChannels.Add(guild.Id, channels);
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
            _ = guild.GetUserAsync(DiscordModule._client.CurrentUser.Id).GetAwaiter().GetResult()
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

        public void Add(ulong key, object value)
        {
            DiscordCacheItem item = new DiscordCacheItem(key, value, Timeout);
            Items.RemoveAll(x => x.Key == key);
            Items.Add(item);

            Items.RemoveAll(x => x.Expiry < DateTime.Now);
        }

        public void Remove(ulong key)
        {
            Items.RemoveAll(x => x.Key == key);
        }

        public bool TryGet(ulong key, out object value)
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
        public ulong Key { get; }
        public DateTime Expiry { get; }
        public object Value { get; }

        public DiscordCacheItem(ulong key, object value, TimeSpan timeout)
        {
            Key = key;
            Expiry = DateTime.Now.Add(timeout);
            Value = value;
        }
    }
}
