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

            _cacheTimer = new Timer(60000);
            _cacheTimer.Elapsed += _cacheTimer_Elapsed;
            _cacheTimer.Start();
        }

        private static void _cacheTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (KeyValuePair<ulong, (DiscordRestClient, DateTime)> cachedUser in _cachedUsers.ToList().Where(x => DateTime.Now - x.Value.Item2 > TimeSpan.FromMinutes(15)))
            {
                cachedUser.Value.Item1.LogoutAsync().GetAwaiter().GetResult();
                _cachedUsers.Remove(cachedUser.Key);
            }
        }

        private static Dictionary<ulong, (DiscordRestClient, DateTime)> _cachedUsers = new Dictionary<ulong, (DiscordRestClient, DateTime)>();

        public static DiscordRestClient GetClient(ulong userId, string token = null)
        {
            if (_cachedUsers.TryGetValue(userId, out (DiscordRestClient, DateTime) cachedClient))
            {
                if (cachedClient.Item1.LoginState == LoginState.LoggedIn)
                {
                    cachedClient.Item2 = DateTime.Now;
                    return cachedClient.Item1;
                }
                
                _cachedUsers.Remove(userId);

                try
                {
                    cachedClient.Item1.LoginAsync(TokenType.Bearer, token).GetAwaiter().GetResult();
                    _cachedUsers.Add(userId, (cachedClient.Item1, DateTime.Now));
                    return cachedClient.Item1;
                }
                catch{}
            }

            DiscordRestClient client = new DiscordRestClient();
            client.LoginAsync(TokenType.Bearer, token).GetAwaiter().GetResult();

            _cachedUsers.Add(userId, (client, DateTime.Now));
            return client;
        }

        public static List<RestUserGuild> GetManageableGuilds(DiscordRestClient client)
        {
            return client.GetGuildSummariesAsync().FlattenAsync().GetAwaiter().GetResult().Where(x => x.Permissions.ManageGuild).ToList();
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
}
