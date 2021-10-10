using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace UtiliBackend.Services
{
    public class DiscordClientService
    {
        private readonly Dictionary<string, DiscordRestClient> _clients;
        
        public DiscordClientService()
        {
            _clients = new();
        }

        public async ValueTask<DiscordRestClient> GetClientAsync(HttpContext httpContext)
        {
            var token = await httpContext.GetTokenAsync("Discord", "access_token");
            
            if (!httpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(token))
                return null;
            
            lock (_clients)
            {
                if (_clients.TryGetValue(token, out var client) && client.LoginState == LoginState.LoggedIn)
                    return client;
            }
            
            var newClient = new DiscordRestClient();
            try
            {
                await newClient.LoginAsync(TokenType.Bearer, token);
            }
            catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            lock (_clients)
            {
                _clients.Remove(token);
                _clients.Add(token, newClient);
            }

            return newClient;
        }
    }
}
