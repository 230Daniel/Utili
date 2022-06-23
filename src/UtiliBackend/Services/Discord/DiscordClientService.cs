using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Http;
using Disqord.OAuth2;
using Disqord.Rest;
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
            var identity = httpContext.User.Identities.FirstOrDefault(x => x.AuthenticationType == "Discord");
            var token = await httpContext.GetTokenAsync("Discord", "access_token");

            if (identity is null || !identity.IsAuthenticated || string.IsNullOrWhiteSpace(token))
                return null;

            // Workaround Snowflake.Parse broken in v191
            // TODO: Replace with Snowflake.Parse when fixed
            var userId = new Snowflake(ulong.Parse(httpContext.User.FindFirstValue("id")));

            await _semaphore.WaitAsync();

            try
            {
                if (_clients.TryGetValue(userId, out var cachedClient))
                {
                    if (cachedClient.Token == token && cachedClient.Authorization.ExpiresAt > DateTimeOffset.Now)
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
            catch (RestApiException ex) when (ex.StatusCode == HttpResponseStatusCode.Unauthorized)
            {
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public class BearerClient
        {
            public IBearerClient Client { get; init; }
            public IBearerAuthorization Authorization { get; init; }
            public ICurrentUser User { get; init; }
            public string Token => Client.RestClient.ApiClient.Token.RawValue;
        }
    }
}
