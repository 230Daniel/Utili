using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Discord;

namespace UtiliSite
{
    public class DiscordModule
    {
        public static DiscordRestClient _client;

        public static void Initialise()
        {
            _client = new DiscordRestClient(new DiscordRestConfig
            {
                LogLevel = LogSeverity.Info,
                
            });
            _client.LoginAsync(TokenType.Bearer, "");
        }
    }
}
