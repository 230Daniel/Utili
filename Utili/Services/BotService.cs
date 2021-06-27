using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Database.Data;
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
        private readonly ILogger<BotService> _logger;
        private readonly IConfiguration _config;
        private readonly DiscordClientBase _client;

        private readonly CommunityService _community;
        private readonly GuildCountService _guildCount;

        private readonly AutopurgeService _autopurge;
        private readonly ChannelMirroringService _channelMirroring;
        private readonly InactiveRoleService _inactiveRole;
        private readonly JoinMessageService _joinMessage;
        private readonly JoinRolesService _joinRoles;
        private readonly MessageFilterService _messageFilter;
        private readonly MessageLogsService _messageLogs;
        private readonly NoticesService _notices;
        private readonly ReputationService _reputation;
        private readonly RoleLinkingService _roleLinking;
        private readonly RolePersistService _rolePersist;
        private readonly VoiceLinkService _voiceLink;
        private readonly VoiceRolesService _voiceRoles;
        private readonly VoteChannelsService _voteChannels;

        public BotService(
            
            ILogger<BotService> logger,
            IConfiguration config,
            DiscordClientBase client, 
            
            CommunityService community,
            GuildCountService guildCount,
            
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
            
            _community = community;
            _guildCount = guildCount;
            
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
            Database.Status.Start();
            _logger.LogInformation("Database initialised");

            await Client.WaitUntilReadyAsync(cancellationToken);

            return;
            _autopurge.Start();
            _inactiveRole.Start();
            _joinRoles.Start();
            _notices.Start();
            _voiceLink.Start();
            _voiceRoles.Start();
            _guildCount.Start();

            _logger.LogInformation("Services started");
        }

        private async Task DownloadMembersAsync(IEnumerable<Snowflake> shardGuildIds = null)
        {
            var guildIds = new List<ulong>();
            guildIds.AddRange((await RolePersist.GetRowsAsync()).Where(x => x.Enabled).Select(x => x.GuildId));
            guildIds.AddRange((await RoleLinking.GetRowsAsync()).Select(x => x.GuildId));
            if (shardGuildIds is not null) guildIds.RemoveAll(x => !shardGuildIds.Contains(x));
            
            foreach (var guildId in guildIds.Distinct())
            {
                var guild = _client.GetGuild(guildId);
                _ = _client.Chunker.ChunkAsync(guild);
            }
        }

        protected override async ValueTask OnReady(ReadyEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                await DownloadMembersAsync(e.GuildIds);
            });
        }

        protected override async ValueTask OnGuildAvailable(GuildAvailableEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                return;
                _ = _community.GuildAvailable(e);
            });
        }

        protected override async ValueTask OnMessageReceived(MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                return;
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
                return;
                _ = _messageLogs.MessageUpdated(e);
                _ = _autopurge.MessageUpdated(e);
            });
        }

        protected override async ValueTask OnMessageDeleted(MessageDeletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                return;
                _ = _messageLogs.MessageDeleted(e);
                _ = _autopurge.MessageDeleted(e);
            });
        }
    
        protected override async ValueTask OnMessagesDeleted(MessagesDeletedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                return;
                _ = _messageLogs.MessagesDeleted(e);
                _ = _autopurge.MessagesDeleted(e);
            });
        }

        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                return;
                _ = _reputation.ReactionAdded(e);
            });
        }
        
        protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                return;
                _ = _reputation.ReactionRemoved(e);
            });
        }

        protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                return;
                _ = _voiceLink.VoiceStateUpdated(e);
                _ = _voiceRoles.VoiceStateUpdated(e);
                _ = _inactiveRole.VoiceStateUpdated(e);
            });
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                return;
                _ = _joinMessage.MemberJoined(e);
                await _joinRoles.MemberJoined(e);
                await Task.Delay(1000); // Delay to ensure that member is updated in cache before getting the member's roles in the next handler
                await _rolePersist.MemberJoined(e);
            });
        }

        protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                return;
                await _joinRoles.MemberUpdated(e);
                await _roleLinking.MemberUpdated(e);
            });
        }

        protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        {
            var member = _client.GetMember(e.GuildId, e.User.Id);
            _ = Task.Run(async () =>
            {
                await _rolePersist.MemberLeft(e, member);
            });
        }
    }
}
