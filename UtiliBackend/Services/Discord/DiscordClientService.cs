using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace UtiliBackend.Services
{
    public class DiscordClientService
    {
        private readonly IBearerClientFactory _bearerClientFactory;
        private readonly ConcurrentDictionary<Snowflake, BearerClient> _clients;
        private readonly SemaphoreSlim _semaphore;

        public DiscordClientService(IBearerClientFactory bearerClientFactory)
        {
            _bearerClientFactory = bearerClientFactory;
            _clients = new();
            _semaphore = new(1, 1);
        }

        public async ValueTask<BearerClient> GetClientAsync(HttpContext httpContext)
        {
            var token = await httpContext.GetTokenAsync("Discord", "access_token");
            
            if (!httpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(token))
                return null;
            
            var userId = Snowflake.Parse(httpContext.User.FindFirstValue("id"));

            await _semaphore.WaitAsync();
            
            try
            {
                if (_clients.TryGetValue(userId, out var cachedClient))
                {
                    if (cachedClient.Authorization.ExpiresAt > DateTimeOffset.Now)
                        return cachedClient;
                    _clients.TryRemove(userId, out _);
                }
                
                var newClient = _bearerClientFactory.CreateClient(Token.Bearer(token));
                var authorisation = await newClient.FetchCurrentAuthorizationAsync();
                var user = await newClient.FetchCurrentUserAsync();

                var client = new BearerClient
                {
                    Client = newClient,
                    Authorization = authorisation,
                    User = user
                };

                _clients.TryAdd(userId, client);
                return client;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public class BearerClient
        {
            public IBearerClient Client { get; set; }
            public IBearerAuthorization Authorization { get; set; }
            public ICurrentUser User { get; set; }
        }
    }
}
