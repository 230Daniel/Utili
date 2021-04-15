using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
        JoinRolesService _joinRoles;
        MessageFilterService _messageFilter;
        MessageLogsService _messageLogs;
        VoiceLinkService _voiceLink;

        public BotService(
            ILogger<BotService> logger, 
            IConfiguration config,
            DiscordClientBase client, 
            AutopurgeService autopurge,
            ChannelMirroringService channelMirroring,
            JoinMessageService joinMessage,
            JoinRolesService joinRoles,
            MessageFilterService messageFilter,
            MessageLogsService messageLogs,
            VoiceLinkService voiceLink)
            : base(logger, client)
        {
            _logger = logger;
            _config = config;
            _client = client;

            _autopurge = autopurge;
            _channelMirroring = channelMirroring;
            _joinMessage = joinMessage;
            _joinRoles = joinRoles;
            _messageFilter = messageFilter;
            _messageLogs = messageLogs;
            _voiceLink = voiceLink;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Database.Database.InitialiseAsync(false, _config.GetValue<string>("defaultPrefix"));
            await Client.WaitUntilReadyAsync(cancellationToken);
            Logger.LogInformation("Ready");

            _client.MessageReceived += _autopurge.MessageReceived;
            _client.MessageReceived += _channelMirroring.MessageReceived;
            _client.MessageReceived += _messageFilter.MessageReceived;
            _client.MessageReceived += _messageLogs.MessageReceived;

            _client.MessageUpdated += _autopurge.MessageUpdated;
            _client.MessageUpdated += _messageLogs.MessageUpdated;

            _client.MessageDeleted += _messageLogs.MessageDeleted;

            _client.MessagesDeleted += _messageLogs.MessagesDeleted;

            _client.VoiceStateUpdated += _voiceLink.VoiceStateUpdated;

            _client.MemberJoined += _joinMessage.MemberJoined;
            _client.MemberJoined += _joinRoles.MemberJoined;

            _client.MemberUpdated += _joinRoles.MemberUpdated;

            _autopurge.Start();
            _joinRoles.Start();
            _voiceLink.Start();
        }
    }
}
