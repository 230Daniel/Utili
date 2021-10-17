using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Gateway;
using Disqord.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Database.Entities;
using Disqord.Rest;
using Microsoft.Extensions.Configuration;
using Utili.Extensions;
using Utili.Features;
using Utili.Utils;

namespace Utili.Services
{
    public class BotService : DiscordClientService
    {
        private readonly ILogger<BotService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly CommunityService _community;
        private readonly GuildCountService _guildCount;
        private readonly MemberCacheService _memberCache;
        
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
            IConfiguration configuration,
            IServiceScopeFactory scopeFactory,
            DiscordBotBase client,
            
            CommunityService community,
            GuildCountService guildCount,
            MemberCacheService memberCache,
            
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
            _configuration = configuration;
            _scopeFactory = scopeFactory;
            
            _community = community;
            _guildCount = guildCount;
            _memberCache = memberCache;
            
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
            await Client.WaitUntilReadyAsync(cancellationToken);

            _memberCache.Start();
            _autopurge.Start();
            _inactiveRole.Start();
            _joinRoles.Start();
            _messageLogs.Start();
            _notices.Start();
            _voiceLink.Start();
            _voiceRoles.Start();
            _guildCount.Start();

            _logger.LogInformation("Services started");
        }

        protected override async ValueTask OnReady(ReadyEventArgs e)
        {
            _ = _memberCache.Ready(e);
        }

        protected override async ValueTask OnGuildAvailable(GuildAvailableEventArgs e)
        {
            _ = _community.GuildAvailable(e);
        }

        protected override async ValueTask OnMessageReceived(MessageReceivedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            
            using var scope = _scopeFactory.CreateScope();
            var config = await scope.GetCoreConfigurationAsync(e.GuildId.Value);
            if (config is null) return;
            
            if(config.HasFeature(BotFeatures.MessageLogs)) 
                await _messageLogs.MessageReceived(scope, e);
            
            if(config.HasFeature(BotFeatures.MessageFilter)) 
                if(await _messageFilter.MessageReceived(scope, e)) return;

            if (e.Channel is not IThreadChannel)
            {
                if(config.HasFeature(BotFeatures.Notices))
                    await _notices.MessageReceived(scope, e);
            
                if(config.HasFeature(BotFeatures.VoteChannels))
                    await _voteChannels.MessageReceived(scope, e);
            
                if(config.HasFeature(BotFeatures.ChannelMirroring))
                    await _channelMirroring.MessageReceived(scope, e);
            
                if(config.HasFeature(BotFeatures.Autopurge))
                    await _autopurge.MessageReceived(scope, e);
            }
            
            if(config.HasFeature(BotFeatures.InactiveRole))
                await _inactiveRole.MessageReceived(scope, e);
        }

        protected override async ValueTask OnMessageUpdated(MessageUpdatedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            
            using var scope = _scopeFactory.CreateScope();
            var config = await scope.GetCoreConfigurationAsync(e.GuildId.Value);
            if (config is null) return;
            
            if(config.HasFeature(BotFeatures.MessageLogs))
                await _messageLogs.MessageUpdated(scope, e);

            if (Client.GetMessageGuildChannel(e.GuildId.Value, e.ChannelId) is not IThreadChannel)
            {
                if(config.HasFeature(BotFeatures.Autopurge))
                    await _autopurge.MessageUpdated(scope, e);
            }
        }

        protected override async ValueTask OnMessageDeleted(MessageDeletedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            
            using var scope = _scopeFactory.CreateScope();
            var config = await scope.GetCoreConfigurationAsync(e.GuildId.Value);
            if (config is null) return;
            
            if(config.HasFeature(BotFeatures.MessageLogs))
                await _messageLogs.MessageDeleted(scope, e);
            
            if (Client.GetMessageGuildChannel(e.GuildId.Value, e.ChannelId) is not IThreadChannel)
            {
                if(config.HasFeature(BotFeatures.Autopurge))
                    await _autopurge.MessageDeleted(scope, e);
            }
        }
    
        protected override async ValueTask OnMessagesDeleted(MessagesDeletedEventArgs e)
        {
            using var scope = _scopeFactory.CreateScope();
            var config = await scope.GetCoreConfigurationAsync(e.GuildId);
            if (config is null) return;
            
            if(config.HasFeature(BotFeatures.MessageLogs))
                await _messageLogs.MessagesDeleted(scope, e);

            if (Client.GetMessageGuildChannel(e.GuildId, e.ChannelId) is not IThreadChannel)
            {
                if(config.HasFeature(BotFeatures.Autopurge))
                    await _autopurge.MessagesDeleted(scope, e);
            }
        }

        protected override async ValueTask OnReactionAdded(ReactionAddedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            
            using var scope = _scopeFactory.CreateScope();
            var config = await scope.GetCoreConfigurationAsync(e.GuildId.Value);
            if (config is null) return;
            
            if(config.HasFeature(BotFeatures.Reputation))
                await _reputation.ReactionAdded(scope, e);
        }
        
        protected override async ValueTask OnReactionRemoved(ReactionRemovedEventArgs e)
        {
            if (!e.GuildId.HasValue) return;
            
            using var scope = _scopeFactory.CreateScope();
            var config = await scope.GetCoreConfigurationAsync(e.GuildId.Value);
            if (config is null) return;
            
            if(config.HasFeature(BotFeatures.Reputation))
                await _reputation.ReactionRemoved(scope, e);
        }

        protected override async ValueTask OnVoiceStateUpdated(VoiceStateUpdatedEventArgs e)
        {
            using var scope = _scopeFactory.CreateScope();
            var config = await scope.GetCoreConfigurationAsync(e.GuildId);
            if (config is null) return;
            
            if(config.HasFeature(BotFeatures.VoiceLink))
                await _voiceLink.VoiceStateUpdated(scope, e);
            
            if(config.HasFeature(BotFeatures.VoiceRoles))
                await _voiceRoles.VoiceStateUpdated(e);
            
            if(config.HasFeature(BotFeatures.InactiveRole))
                await _inactiveRole.VoiceStateUpdated(scope, e);
        }

        protected override async ValueTask OnMemberJoined(MemberJoinedEventArgs e)
        {
            using var scope = _scopeFactory.CreateScope();
            var config = await scope.GetCoreConfigurationAsync(e.GuildId);
            if (config is null) return;
            
            if(config.HasFeature(BotFeatures.RolePersist))
                await _rolePersist.MemberJoined(scope, e);
            
            if(config.HasFeature(BotFeatures.JoinRoles))
                await _joinRoles.MemberJoined(scope, e);
            
            if(config.HasFeature(BotFeatures.JoinMessage))
                await _joinMessage.MemberJoined(scope, e);
        }

        protected override async ValueTask OnMemberUpdated(MemberUpdatedEventArgs e)
        {
            using var scope = _scopeFactory.CreateScope();
            var config = await scope.GetCoreConfigurationAsync(e.NewMember.GuildId);
            if (config is null) return;
            
            if(config.HasFeature(BotFeatures.JoinRoles))
                await _joinRoles.MemberUpdated(scope, e);
            
            if(config.HasFeature(BotFeatures.RoleLinking))
                await _roleLinking.MemberUpdated(scope, e);
        }

        protected override async ValueTask OnMemberLeft(MemberLeftEventArgs e)
        {
            using var scope = _scopeFactory.CreateScope();
            var config = await scope.GetCoreConfigurationAsync(e.GuildId);
            if (config is null) return;
            
            var member = e.User is IMember user ? user : null;
            
            if(config.HasFeature(BotFeatures.RolePersist))
                await _rolePersist.MemberLeft(scope, e, member);
        }

        protected override async ValueTask OnJoinedGuild(JoinedGuildEventArgs e)
        {
            ITextChannel idealChannel = null;
            
            foreach (var channelId in new []
            {
                e.Guild.PublicUpdatesChannelId,
                e.Guild.SystemChannelId
            })
            {
                if(!channelId.HasValue) continue;
                var channel = e.Guild.GetTextChannel(channelId.Value);
                if(!channel.BotHasPermissions(Permission.ViewChannels | Permission.SendMessages | Permission.SendEmbeds)) continue;
                idealChannel = channel;
                break;
            }

            idealChannel ??= e.Guild.Channels.Values
                .OfType<ITextChannel>()
                .Where(x => x.BotHasPermissions(Permission.ViewChannels | Permission.SendMessages | Permission.SendEmbeds))
                .OrderBy(x => x.CreatedAt())
                .FirstOrDefault();
            
            if (idealChannel is null) return;

            var baseUrl = $"https://{_configuration["Domain"]}";
            await idealChannel.SendMessageAsync(
                new LocalMessage()
                    .AddEmbed(
                        MessageUtils.CreateEmbed(
                            EmbedType.Info, 
                            "Hello! Thanks for choosing Utili.", 
                            $"Head to the [dashboard]({baseUrl}/dashboard/{e.GuildId}) to configure the bot.\n" +
                            $"If you need any help, you should [contact us]({baseUrl}/contact).\n" +
                            $"And if you want to help support the bot, you can [get premium]({baseUrl}/premium).\n\n" +
                            $"[Invite]({baseUrl}/invite) • [Terms]({baseUrl}/terms) • [Privacy]({baseUrl}/privacy) • [Contact]({baseUrl}/contact)")));
        }
    }
}
