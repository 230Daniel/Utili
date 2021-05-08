using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Hosting;
using Disqord.Rest;
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

        RoleCacheService _roleCache;
        CommunityService _community;
        
        AutopurgeService _autopurge;
        ChannelMirroringService _channelMirroring;
        InactiveRoleService _inactiveRole;
        JoinMessageService _joinMessage;
        JoinRolesService _joinRoles;
        MessageFilterService _messageFilter;
        MessageLogsService _messageLogs;
        NoticesService _notices;
        ReputationService _reputation;
        RoleLinkingService _roleLinking;
        RolePersistService _rolePersist;
        VoiceLinkService _voiceLink;
        VoiceRolesService _voiceRoles;
        VoteChannelsService _voteChannels;

        public BotService(
            ILogger<BotService> logger, 
            IConfiguration config,
            RoleCacheService roleCache,
            CommunityService community,
            DiscordClientBase client, 
            AutopurgeService autopurge,
            ChannelMirroringService channelMirroring,
            InactiveRoleService inactiveRole,
            JoinMessageService joinMessage,
            JoinRolesService joinRoles,
            MessageFilterService messageFilter,
            MessageLogsService messageLogs,
            NoticesService notices,
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

            _roleCache = roleCache;
            _community = community;
            
            _autopurge = autopurge;
            _channelMirroring = channelMirroring;
            _inactiveRole = inactiveRole;
            _joinMessage = joinMessage;
            _joinRoles = joinRoles;
            _messageFilter = messageFilter;
            _messageLogs = messageLogs;
            _notices = notices;
            _reputation = reputation;
            _roleLinking = roleLinking;
            _rolePersist = rolePersist;
            _voiceLink = voiceLink;
            _voiceRoles = voiceRoles;
            _voteChannels = voteChannels;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Database.Database.InitialiseAsync(false, _config.GetValue<string>("DefaultPrefix"));
            _logger.LogInformation("Database initialised");

            await Client.WaitUntilReadyAsync(cancellationToken);

            _autopurge.Start();
            _inactiveRole.Start();
            _joinRoles.Start();
            _notices.Start();
            _voiceLink.Start();
            _voiceRoles.Start();
            
            _logger.LogInformation("Services started");
        }

        protected override async ValueTask OnReady(ReadyEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                _ = _roleCache.Ready(e);
                _ = _community.Ready(e);
            });
        }

        protected override async ValueTask OnGuildAvailable(GuildAvailableEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                _ = _community.GuildAvailable(e);
            });
        }

        protected override async ValueTask OnMessageReceived(MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await _messageLogs.MessageReceived(e);
                if(await _messageFilter.MessageReceived(e)) return;
                _ = _notices.MessageReceived(e);
                _ = _voteChannels.MessageReceived(e);
                _ = _channelMirroring.MessageReceived(e);
                _ = _autopurge.MessageReceived(e);
                _ = _inactiveRole.MessageReceived(e);
            });
            
        }

        protected override async ValueTask OnMessageUpdated(MessageUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                _ = _messageLogs.MessageUpdated(e);
                _ = _autopurge.MessageUpdated(e);
            });
        }

        protected override async ValueTask OnMessageDeleted(MessageDeletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                _ = _messageLogs.MessageDeleted(e);
            });
        }
    
        protected override async ValueTask OnMessagesDeleted(MessagesDeletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                _ = _messageLogs.MessagesDeleted(e);
            });
        }

        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                _ = _reputation.ReactionAdded(e);
            });
        }
        
        protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                _ = _reputation.ReactionRemoved(e);
            });
        }

        protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                _ = _voiceLink.VoiceStateUpdated(e);
                _ = _voiceRoles.VoiceStateUpdated(e);
                _ = _inactiveRole.VoiceStateUpdated(e);
            });
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                _ = _rolePersist.MemberJoined(e);
                _ = _joinMessage.MemberJoined(e);
                _ = _joinRoles.MemberJoined(e);
            });
        }

        protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await _joinRoles.MemberUpdated(e);
                await _roleLinking.MemberUpdated(e);
                _ = _roleCache.MemberUpdated(e);
            });
        }

        protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await _rolePersist.MemberLeft(e);
                _ = _roleCache.MemberLeft(e);
            });
        }
    }
}
