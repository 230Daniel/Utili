using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Gateway;
using Microsoft.Extensions.Configuration;

namespace Utili.Services
{
    class PrefixProvider : IPrefixProvider
    {
        IConfiguration _config;

        public PrefixProvider(IConfiguration config)
        {
            _config = config;
        }

        public ValueTask<IEnumerable<IPrefix>> GetPrefixesAsync(IGatewayUserMessage message)
        {
            return new ValueTask<IEnumerable<IPrefix>>(new List<IPrefix>
            {
                new StringPrefix(_config.GetValue<string>("prefix"))
            });
        }
    }
}
