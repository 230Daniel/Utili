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

        public static DiscordRestClient Login(string token)
        {
            DiscordRestClient client = new DiscordRestClient();
            client.LoginAsync(TokenType.Bearer, token).GetAwaiter().GetResult();

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
