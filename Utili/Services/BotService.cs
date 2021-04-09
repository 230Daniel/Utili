using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Hosting;
using Microsoft.Extensions.Logging;

namespace Utili.Services
{
    public class BotService : DiscordClientService
    {
        public static DiscordClientBase Client { get; protected set; }

        public BotService(ILogger<BotService> logger, DiscordClientBase client)
            : base(logger, client)
        {
            Client = client;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Client.WaitUntilReadyAsync(cancellationToken);
            Logger.LogInformation("Client says it's ready which is really cool.");

            // write cache-dependent code here
        }
    }
}
