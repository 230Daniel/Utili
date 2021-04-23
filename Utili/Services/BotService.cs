using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
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
        InactiveRoleService _inactiveRole;
        JoinMessageService _joinMessage;
        JoinRolesService _joinRoles;
        MessageFilterService _messageFilter;
        MessageLogsService _messageLogs;
        ReputationService _reputation;
        RoleLinkingService _roleLinking;
        RolePersistService _rolePersist;
        VoiceLinkService _voiceLink;
        VoiceRolesService _voiceRoles;
        VoteChannelsService _voteChannels;

        public BotService(
            ILogger<BotService> logger, 
            IConfiguration config,
            DiscordClientBase client, 
            AutopurgeService autopurge,
            ChannelMirroringService channelMirroring,
            InactiveRoleService inactiveRole,
            JoinMessageService joinMessage,
            JoinRolesService joinRoles,
            MessageFilterService messageFilter,
            MessageLogsService messageLogs,
            ReputationService reputation,
            RoleLinkingService roleLinking,
            RolePersistService rolePersist,
            VoiceLinkService voiceLink,
            VoiceRolesService voiceRoles,
            VoteChannelsService voteChannels)
            : base(logger, client)
        {
            _logger = logger;
            _config = config;
            _client = client;

            _autopurge = autopurge;
            _channelMirroring = channelMirroring;
            _inactiveRole = inactiveRole;
            _joinMessage = joinMessage;
            _joinRoles = joinRoles;
            _messageFilter = messageFilter;
            _messageLogs = messageLogs;
            _reputation = reputation;
            _roleLinking = roleLinking;
            _rolePersist = rolePersist;
            _voiceLink = voiceLink;
            _voiceRoles = voiceRoles;
            _voteChannels = voteChannels;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Database.Database.InitialiseAsync(false, _config.GetValue<string>("defaultPrefix"));
            _logger.LogInformation("Database initialised");

            await Client.WaitUntilReadyAsync(cancellationToken);

            _autopurge.Start();
            _inactiveRole.Start();
            _joinRoles.Start();
            _voiceLink.Start();
            _voiceRoles.Start();
            
            _logger.LogInformation("Services started");
        }

        protected override async ValueTask OnMessageReceived(MessageReceivedEventArgs e)
        {
            await Task.Yield();
            
            await _messageLogs.MessageReceived(e);
            if(await _messageFilter.MessageReceived(e)) return;
            await _voteChannels.MessageReceived(e);
            await _channelMirroring.MessageReceived(e);
            await _autopurge.MessageReceived(e);
            await _inactiveRole.MessageReceived(e);
        }

        protected override async ValueTask OnMessageUpdated(MessageUpdatedEventArgs e)
        {
            await Task.Yield();
            
            await _messageLogs.MessageUpdated(e);
            await _autopurge.MessageUpdated(e);
        }

        protected override async ValueTask OnMessageDeleted(MessageDeletedEventArgs e)
        {
            await Task.Yield();
            
            await _messageLogs.MessageDeleted(e);
        }
    
        protected override async ValueTask OnMessagesDeleted(MessagesDeletedEventArgs e)
        {
            await Task.Yield();
            
            await _messageLogs.MessagesDeleted(e);
        }

        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            await Task.Yield();
            
            await _reputation.ReactionAdded(e);
        }
        
        protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
        {
            await Task.Yield();
            
            await _reputation.ReactionRemoved(e);
        }

        protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            await Task.Yield();
            
            await _voiceLink.VoiceStateUpdated(e);
            await _voiceRoles.VoiceStateUpdated(e);
            await _inactiveRole.VoiceStateUpdated(e);
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            await Task.Yield();
            
            await _rolePersist.MemberJoined(e);
            await _joinMessage.MemberJoined(e);
            await _joinRoles.MemberJoined(e);
        }

        protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
        {
            await Task.Yield();
            
            await _joinRoles.MemberUpdated(e);
            await _roleLinking.MemberUpdated(e);
        }

        protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        {
            await Task.Yield();
            
            await _rolePersist.MemberLeft(e);
        }
    }
}
