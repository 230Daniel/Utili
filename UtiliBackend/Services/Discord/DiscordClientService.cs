using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
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
        private readonly Dictionary<ulong, DiscordRestClient> _clients;
        
        public DiscordClientService()
        {
            _clients = new();
        }

        public async ValueTask<DiscordRestClient> GetClientAsync(HttpContext httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return null;
            
            var userId = ulong.Parse(httpContext.User.FindFirstValue("id"));

            lock (_clients)
            {
                if (_clients.TryGetValue(userId, out var client) && client.LoginState == LoginState.LoggedIn)
                    return client;
            }

            var newClient = new DiscordRestClient();
            try
            {
                var token = await httpContext.GetTokenAsync("Discord", "access_token");
                await newClient.LoginAsync(TokenType.Bearer, token);
            }
            catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }

            lock (_clients)
            {
                _clients.Remove(userId);
                _clients.Add(userId, newClient);
            }

            return newClient;
        }
    }
}
