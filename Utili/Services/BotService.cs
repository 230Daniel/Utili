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
        ReputationService _reputation;
        RoleLinkingService _roleLinking;
        RolePersistService _rolePersist;
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
            ReputationService reputation,
            RoleLinkingService roleLinking,
            RolePersistService rolePersist,
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
            _reputation = reputation;
            _roleLinking = roleLinking;
            _rolePersist = rolePersist;
            _voiceLink = voiceLink;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Database.Database.InitialiseAsync(false, _config.GetValue<string>("defaultPrefix"));
            _logger.LogInformation("Database initialised");

            await Client.WaitUntilReadyAsync(cancellationToken);

            _client.MessageReceived += _autopurge.MessageReceived;
            _client.MessageReceived += _channelMirroring.MessageReceived;
            _client.MessageReceived += _messageFilter.MessageReceived;
            _client.MessageReceived += _messageLogs.MessageReceived;

            _client.MessageUpdated += _autopurge.MessageUpdated;
            _client.MessageUpdated += _messageLogs.MessageUpdated;

            _client.MessageDeleted += _messageLogs.MessageDeleted;

            _client.MessagesDeleted += _messageLogs.MessagesDeleted;

            _client.ReactionAdded += _reputation.ReactionAdded;

            _client.ReactionRemoved += _reputation.ReactionRemoved;

            _client.VoiceStateUpdated += _voiceLink.VoiceStateUpdated;

            _client.MemberJoined += _joinMessage.MemberJoined;
            _client.MemberJoined += _joinRoles.MemberJoined;
            _client.MemberJoined += _rolePersist.MemberJoined;

            _client.MemberUpdated += _joinRoles.MemberUpdated;
            _client.MemberUpdated += _roleLinking.MemberUpdated;

            _client.MemberLeft += _rolePersist.MemberLeft;

            _logger.LogInformation("All events registered");

            _autopurge.Start();
            _joinRoles.Start();
            _voiceLink.Start();

            _logger.LogInformation("All services started");
        }
    }
}
