using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Utili.Features;

namespace Utili.Services
{
    public class BotService : DiscordClientService
    {
        ILogger<BotService> _logger;
        IConfiguration _config;
        DiscordClientBase _client;

        AutopurgeService _autopurge;
        ChannelMirroringService _channelMirroring;
        JoinMessageService _joinMessage;
        VoiceLinkService _voiceLink;

        public BotService(ILogger<BotService> logger, 
            IConfiguration config,
            DiscordClientBase client, 
            AutopurgeService autopurge,
            ChannelMirroringService channelMirroring,
            JoinMessageService joinMessage,
            VoiceLinkService voiceLink)
            : base(logger, client)
        {
            _logger = logger;
            _config = config;
            _client = client;

            _autopurge = autopurge;
            _channelMirroring = channelMirroring;
            _joinMessage = joinMessage;
            _voiceLink = voiceLink;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Database.Database.InitialiseAsync(false, _config.GetValue<string>("defaultPrefix"));
            await Client.WaitUntilReadyAsync(cancellationToken);
            Logger.LogInformation("Ready");

            _client.MessageReceived += _autopurge.MessageReceived;
            _client.MessageReceived += _channelMirroring.MessageReceived;
            _client.MessageUpdated += _autopurge.MessageUpdated;
            _client.VoiceStateUpdated += _voiceLink.VoiceStateUpdated;
            _client.MemberJoined += _joinMessage.MemberJoined;

            _autopurge.Start();
            _voiceLink.Start();
        }
    }
}
