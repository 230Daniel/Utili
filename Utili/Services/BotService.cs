using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Hosting;
using Microsoft.Extensions.Logging;
using Utili.Features;

namespace Utili.Services
{
    public class BotService : DiscordClientService
    {
        ILogger<BotService> _logger;
        DiscordClientBase _client;

        AutopurgeService _autopurge;
        VoiceLinkService _voiceLink;

        public BotService(ILogger<BotService> logger, 
            DiscordClientBase client, 
            AutopurgeService autopurge,
            VoiceLinkService voiceLink)
            : base(logger, client)
        {
            _logger = logger;
            _client = client;
            _autopurge = autopurge;
            _voiceLink = voiceLink;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Database.Database.InitialiseAsync(false, ".");
            await Client.WaitUntilReadyAsync(cancellationToken);
            Logger.LogInformation("Client says it's ready which is really cool.");

            _client.MessageReceived += _autopurge.MessageReceived;
            _client.VoiceStateUpdated += _voiceLink.VoiceStateUpdated;

            _autopurge.Start();
            _voiceLink.Start();
        }
    }
}
