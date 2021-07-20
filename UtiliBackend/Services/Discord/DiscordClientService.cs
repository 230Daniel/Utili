using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UtiliBackend.Services
{
    public class DiscordClientService
    {
        private readonly ILogger<DiscordClientService> _logger;
        private readonly Dictionary<ulong, DiscordRestClient> _clients;
        
        public DiscordClientService(ILogger<DiscordClientService> logger)
        {
            _logger = logger;
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
                {
                    _logger.LogDebug("Returned cached client for user {UserId}", userId);
                    return client;
                }
            }

            _logger.LogDebug("Logging in a new client for user {UserId}", userId);
            
            var newClient = new DiscordRestClient();
            var token = await httpContext.GetTokenAsync("Discord", "access_token");
            await newClient.LoginAsync(TokenType.Bearer, token);

            lock (_clients)
            {
                _clients.Remove(userId);
                _clients.Add(userId, newClient);
            }

            _logger.LogInformation("A client for user {UserId} has been logged in", userId);

            return newClient;
        }
    }
}
