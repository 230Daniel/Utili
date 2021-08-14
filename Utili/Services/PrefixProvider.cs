using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utili.Extensions;

namespace Utili.Services
{
    internal class PrefixProvider : IPrefixProvider
    {
        private IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;

        public PrefixProvider(IConfiguration config, IServiceScopeFactory scopeFactory)
        {
            _config = config;
            _scopeFactory = scopeFactory;
        }

        public async ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(IGatewayUserMessage message)
        {
            if (!message.GuildId.HasValue)
            {
                return new IPrefix[]
                {
                    new StringPrefix(_config.GetValue<string>("defaultPrefix")),
                    new MentionPrefix((message.Client as DiscordClientBase).CurrentUser.Id)
                };
            }

            using var scope = _scopeFactory.CreateScope();
            var config = await scope.GetCoreConfigurationAsync(message.GuildId.Value);
            
            return new IPrefix[]
            {
                new StringPrefix(string.IsNullOrWhiteSpace(config?.Prefix) ? _config.GetValue<string>("defaultPrefix") : config.Prefix),
                new MentionPrefix((message.Client as DiscordClientBase).CurrentUser.Id)
            };
        }
    }
}
