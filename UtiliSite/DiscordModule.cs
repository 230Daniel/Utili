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
        private static Timer _cacheTimer;

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

        private static DiscordCache _cachedGuildLists = new DiscordCache(8);
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

        public void Add(ulong userId, object value)
        {
            DiscordCacheItem item = new DiscordCacheItem(userId, value, Timeout);
            Items.RemoveAll(x => x.UserId == userId);
            Items.Add(item);

            Items.RemoveAll(x => x.Expiry < DateTime.Now);
        }

        public void Remove(ulong userId)
        {
            Items.RemoveAll(x => x.UserId == userId);
        }

        public bool TryGet(ulong userId, out object value)
        {
            value = null;

            Items.RemoveAll(x => x.Expiry < DateTime.Now);

            List<DiscordCacheItem> matches = Items.Where(x => x.UserId == userId).ToList();

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
    }

    internal class DiscordCacheItem
    {
        public ulong UserId { get; }
        public DateTime Expiry { get; }
        public object Value { get; }

        public DiscordCacheItem(ulong userId, object value, TimeSpan timeout)
        {
            UserId = userId;
            Expiry = DateTime.Now.Add(timeout);
            Value = value;
        }
    }
}
