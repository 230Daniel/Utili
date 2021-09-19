using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Microsoft.Extensions.Logging;
using NewDatabase;
using Database.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NewDatabase.Entities;

namespace DatabaseMigrator.Services
{
    
    public class MigratorService
    {
        private static readonly bool UseProdDb = true;
        
        private readonly ILogger<MigratorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly DatabaseContext _db;

        private List<CoreConfiguration> _coreConfigurations;

        public MigratorService(ILogger<MigratorService> logger, IConfiguration configuration, DatabaseContext db)
        {
            _logger = logger;
            _configuration = configuration;
            _db = db;
            _coreConfigurations = new();
        }

        public async Task RunAsync()
        {
            try
            {
                await Database.Database.InitialiseAsync(false, ".", UseProdDb);
                _logger.LogInformation("Old database initialised - Prod: {Prod}", UseProdDb);

                _logger.LogInformation("Migrating core...");
                await MigrateCoreAsync();
                
                _coreConfigurations = await _db.CoreConfigurations.ToListAsync();
                
                _logger.LogInformation("Migrating autopurge...");
                await MigrateAutopurgeAsync();
                
                _logger.LogInformation("Migrating channel mirroring...");
                await MigrateChannelMirroringAsync();
                
                _logger.LogInformation("Migrating inactive role...");
                await MigrateInactiveRoleAsync();

                _logger.LogInformation("Migrating join message...");
                await MigrateJoinMessageAsync();

                _logger.LogInformation("Migrating join roles...");
                await MigrateJoinRolesAsync();

                _logger.LogInformation("Migrating message filter...");
                await MigrateMessageFilterAsync();

                _logger.LogInformation("Migrating message logs...");
                await MigrateMessageLogsAsync();

                _logger.LogInformation("Migrating message pinning...");
                await MigrateMessgePinningAsync();
                
                _logger.LogInformation("Migrating notices...");
                await MigrateNoticesAsync();

                _logger.LogInformation("Migrating premium slots...");
                await MigratePremiumSlotsAsync();

                _logger.LogInformation("Migrating reputation...");
                await MigrateReputationAsync();

                _logger.LogInformation("Migrating role linking...");
                await MigrateRoleLinkingAsync();

                _logger.LogInformation("Migrating role persist...");
                await MigrateRolePersistAsync();

                _logger.LogInformation("Migrating subscriptions...");
                await MigrateSubscriptionsAsync();

                _logger.LogInformation("Migrating users...");
                await MigrateUsersAsync();

                _logger.LogInformation("Mirgating voice link...");
                await MigrateVoiceLinkAsync();

                _logger.LogInformation("Migrating voice roles...");
                await MigrateVoiceRolesAsync();

                _logger.LogInformation("Migrating vote channels...");
                await MigrateVoteChannelsAsync();
                
                _logger.LogInformation("Finished");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while running");
            }
        }
        
        private void SetHasFeature(ulong guildId, BotFeatures feature, bool enabled)
        {
            var coreConfig = _coreConfigurations.FirstOrDefault(x => x.GuildId == guildId);
            if (coreConfig is not null && coreConfig.HasFeature(feature) == enabled) return;
            
            if (coreConfig is null)
            {
                coreConfig = new CoreConfiguration(guildId)
                {
                    Prefix = _configuration["Other:DefaultPrefix"],
                    CommandsEnabled = true,
                    NonCommandChannels = new()
                };
                coreConfig.SetHasFeature(feature, enabled);
                _db.CoreConfigurations.Add(coreConfig);
                _coreConfigurations.Add(coreConfig);
            }
            else
            {
                coreConfig.SetHasFeature(feature, enabled);
                _db.CoreConfigurations.Update(coreConfig);
            }
        }

        private async Task MigrateAutopurgeAsync()
        {
            static AutopurgeMode GetMode(int oldMode)
            {
                return oldMode switch
                {
                    -1 => AutopurgeMode.User,
                    0 => AutopurgeMode.All,
                    1 => AutopurgeMode.Bot,
                    2 => AutopurgeMode.None,
                    3 => AutopurgeMode.User,
                    _ => throw new ArgumentException($"Bad autopurge mode {oldMode}", nameof(oldMode))
                };
            }
            
            var rows = await Autopurge.GetRowsAsync();
            _db.AutopurgeConfigurations.RemoveRange(await _db.AutopurgeConfigurations.ToListAsync());
            
            foreach (var row in rows)
            {
                var autopurgeConfiguration = new AutopurgeConfiguration(row.GuildId, row.ChannelId)
                {
                    Mode = GetMode(row.Mode),
                    Timespan = row.Timespan
                };
                _db.AutopurgeConfigurations.Add(autopurgeConfiguration);
                _logger.LogDebug("Migrated autopurge configuration {GuildId}/{ChannelId}", row.GuildId, row.ChannelId);
                
                SetHasFeature(row.GuildId, BotFeatures.Autopurge, true);
            }
            
            /*var messageRows = await Autopurge.GetMessagesAsync();
            _db.AutopurgeMessages.RemoveRange(await _db.AutopurgeMessages.ToListAsync());
            
            foreach (var messageRow in messageRows)
            {
                var autopurgeMessage = new AutopurgeMessage(messageRow.MessageId)
                {
                    GuildId = messageRow.GuildId,
                    ChannelId = messageRow.ChannelId,
                    Timestamp = messageRow.Timestamp,
                    IsBot = messageRow.IsBot,
                    IsPinned = messageRow.IsPinned
                };
                _db.AutopurgeMessages.Add(autopurgeMessage);
                _logger.LogDebug("Migrated autopurge message {MessageId}", messageRow.MessageId);
            }*/
            
            await _db.SaveChangesAsync();
        }
        
        private async Task MigrateChannelMirroringAsync()
        {
            var rows = await ChannelMirroring.GetRowsAsync();
            _db.ChannelMirroringConfigurations.RemoveRange(await _db.ChannelMirroringConfigurations.ToListAsync());
            
            foreach (var row in rows)
            {
                var channelMirroringConfiguration = new ChannelMirroringConfiguration(row.GuildId, row.FromChannelId)
                {
                    DestinationChannelId = row.ToChannelId,
                    WebhookId = row.WebhookId
                };
                _db.ChannelMirroringConfigurations.Add(channelMirroringConfiguration);
                _logger.LogDebug("Migrated channel mirroring configuration {GuildId}/{ChannelId}", row.GuildId, row.FromChannelId);
                
                SetHasFeature(row.GuildId, BotFeatures.ChannelMirroring, true);
            }
            
            await _db.SaveChangesAsync();
        }
        
        private async Task MigrateCoreAsync()
        {
            var rows = await Core.GetRowsAsync();
            _db.CoreConfigurations.RemoveRange(await _db.CoreConfigurations.ToListAsync());
            
            foreach (var row in rows)
            {
                var coreConfiguration = new CoreConfiguration(row.GuildId)
                {
                    Prefix = row.Prefix.Value,
                    CommandsEnabled = row.EnableCommands,
                    NonCommandChannels = row.ExcludedChannels
                };
                _db.CoreConfigurations.Add(coreConfiguration);
                _logger.LogDebug("Migrated core configuration {GuildId}", row.GuildId);
            }

            await _db.SaveChangesAsync();
        }
        
        private async Task MigrateInactiveRoleAsync()
        {
            var rows = await InactiveRole.GetRowsAsync();
            _db.InactiveRoleConfigurations.RemoveRange(await _db.InactiveRoleConfigurations.ToListAsync());
            
            foreach (var row in rows)
            {
                var inactiveRoleConfiguration = new InactiveRoleConfiguration(row.GuildId)
                {
                    RoleId = row.RoleId,
                    ImmuneRoleId = row.ImmuneRoleId,
                    Threshold = row.Threshold,
                    Mode = row.Inverse ? InactiveRoleMode.RevokeWhenInactive : InactiveRoleMode.GrantWhenInactive,
                    AutoKick = row.AutoKick,
                    AutoKickThreshold = row.AutoKickThreshold,
                    DefaultLastAction = row.DefaultLastAction,
                    LastUpdate = row.LastUpdate
                };
                _db.InactiveRoleConfigurations.Add(inactiveRoleConfiguration);
                _logger.LogDebug("Migrated inactive role configuration {GuildId}", row.GuildId);
                
                SetHasFeature(row.GuildId, BotFeatures.InactiveRole, inactiveRoleConfiguration.RoleId != 0);
            }

            await _db.SaveChangesAsync();
        }
        
        private async Task MigrateJoinMessageAsync()
        {
            var rows = await JoinMessage.GetRowsAsync();
            _db.JoinMessageConfigurations.RemoveRange(await _db.JoinMessageConfigurations.ToListAsync());
            
            foreach (var row in rows)
            {
                var joinMessageConfiguration = new JoinMessageConfiguration(row.GuildId)
                {
                    Enabled = row.Enabled,
                    Mode = row.Direct ? JoinMessageMode.DirectMessage : JoinMessageMode.Channel,
                    ChannelId = row.ChannelId,
                    Title = row.Title.Value,
                    Footer = row.Footer.Value,
                    Content = row.Content.Value,
                    Text = row.Text.Value,
                    Image = row.Image.Value,
                    Thumbnail = row.Thumbnail.Value,
                    Icon = row.Icon.Value,
                    Colour = row.Colour
                };
                _db.JoinMessageConfigurations.Add(joinMessageConfiguration);
                _logger.LogDebug("Migrated join message configuration {GuildId}", row.GuildId);
                
                SetHasFeature(row.GuildId, BotFeatures.JoinMessage, joinMessageConfiguration.Enabled);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateJoinRolesAsync()
        {
            var rows = await JoinRoles.GetRowsAsync();
            _db.JoinRolesConfigurations.RemoveRange(await _db.JoinRolesConfigurations.ToListAsync());

            foreach (var row in rows)
            {
                var joinRolesConfiguration = new JoinRolesConfiguration(row.GuildId)
                {
                    WaitForVerification = row.WaitForVerification,
                    JoinRoles = row.JoinRoles
                };

                _db.JoinRolesConfigurations.Add(joinRolesConfiguration);
                _logger.LogDebug("Migrated join roles configuration {GuildId}", row.GuildId);
                
                SetHasFeature(row.GuildId, BotFeatures.JoinRoles, joinRolesConfiguration.JoinRoles.Any());
            }

            var pendingRows = await JoinRoles.GetPendingRowsAsync();
            _db.JoinRolesPendingMembers.RemoveRange(await _db.JoinRolesPendingMembers.ToListAsync());

            foreach (var pendingRow in pendingRows)
            {
                var joinRolesPendingMember = new JoinRolesPendingMember(pendingRow.GuildId, pendingRow.UserId)
                {
                    IsPending = pendingRow.IsPending,
                    ScheduledFor = pendingRow.ScheduledFor
                };

                _db.JoinRolesPendingMembers.Add(joinRolesPendingMember);
                _logger.LogDebug("Migrated join roles pending member {GuildId}/{MemberId}", pendingRow.GuildId, pendingRow.UserId);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateMessageFilterAsync()
        {
            static MessageFilterMode GetMode(int mode)
            {
                return mode switch
                {
                    0 => MessageFilterMode.All,
                    1 => MessageFilterMode.Images,
                    2 => MessageFilterMode.Videos,
                    3 => MessageFilterMode.Images | MessageFilterMode.Videos,
                    4 => MessageFilterMode.Music,
                    5 => MessageFilterMode.Attachments,
                    6 => MessageFilterMode.Links,
                    7 => MessageFilterMode.Images | MessageFilterMode.Videos | MessageFilterMode.Links,
                    8 => MessageFilterMode.RegEx,
                    _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Bad message filter mode")
                };
            }
            
            var rows = await MessageFilter.GetRowsAsync();
            _db.MessageFilterConfigurations.RemoveRange(await _db.MessageFilterConfigurations.ToListAsync());

            foreach (var row in rows)
            {
                var messageFilterConfiguration = new MessageFilterConfiguration(row.GuildId, row.ChannelId)
                {
                    Mode = GetMode(row.Mode),
                    RegEx = row.Complex.Value
                };

                _db.MessageFilterConfigurations.Add(messageFilterConfiguration);
                _logger.LogDebug("Migrated message filter configuration {GuildId}/{ChannelId}", row.GuildId, row.ChannelId);
                
                SetHasFeature(row.GuildId, BotFeatures.MessageFilter, true);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateMessageLogsAsync()
        {
            var rows = await MessageLogs.GetRowsAsync();
            _db.MessageLogsConfigurations.RemoveRange(await _db.MessageLogsConfigurations.ToListAsync());

            foreach (var row in rows)
            {
                var messageLogsConfiguration = new MessageLogsConfiguration(row.GuildId)
                {
                    DeletedChannelId = row.DeletedChannelId,
                    EditedChannelId = row.EditedChannelId,
                    ExcludedChannels = row.ExcludedChannels
                };

                _db.MessageLogsConfigurations.Add(messageLogsConfiguration);
                _logger.LogDebug("Migrated message logs configuration {GuildId}", row.GuildId);
                
                SetHasFeature(row.GuildId, BotFeatures.MessageLogs, messageLogsConfiguration.DeletedChannelId != 0 || messageLogsConfiguration.EditedChannelId != 0);
            }

            await _db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE message_logs_messages;");
            
            var reader = await Sql.ExecuteReaderAsync("SELECT COUNT(*) FROM MessageLogsMessages");
            reader.Read();
            var rowCount = reader.GetInt32(0) + 1000;
            var chunkCount = rowCount / 1000;

            var tasks = new List<Task<IEnumerable<MessageLogsMessage>>>();
            for (var chunkNum = 0; chunkNum < chunkCount; chunkNum++)
            {
                while (tasks.Count(x => !x.IsCompleted) >= 8)
                    await Task.Delay(1000);

                tasks.Add(MigrateMessageLogsMessagesChunkAsync(chunkNum));
            }

            await Task.WhenAll(tasks);

            var messages = new List<MessageLogsMessage>();
            foreach (var task in tasks)
            {
                messages.AddRange(await task);
            }

            messages = messages
                .GroupBy(x => x.MessageId)
                .Select(x => x.First())
                .ToList();

            _db.MessageLogsMessages.AddRange(messages);
            await _db.SaveChangesAsync();
        }

        private async Task<IEnumerable<MessageLogsMessage>> MigrateMessageLogsMessagesChunkAsync(int chunkNumber)
        {
            var messageRows = new MessageLogsMessageRow[1000];
            var reader = await Sql.ExecuteReaderAsync($"SELECT * FROM MessageLogsMessages ORDER BY MessageId LIMIT 1000 OFFSET {chunkNumber * 1000}");

            var i = 0;
            while (reader.Read())
            {
                messageRows[i] = MessageLogsMessageRow.FromDatabase(
                    reader.GetUInt64(0),
                    reader.GetUInt64(1),
                    reader.GetUInt64(2),
                    reader.GetUInt64(3),
                    reader.GetDateTime(4),
                    reader.GetString(5));
                i++;
            }

            _logger.LogDebug("Fetched message logs messages for chunk {ChunkNumber}", chunkNumber);
            
            return messageRows.Where(x => x is not null).Select(messageRow => new MessageLogsMessage(messageRow.MessageId)
            {
                GuildId = messageRow.GuildId,
                ChannelId = messageRow.ChannelId,
                AuthorId = messageRow.UserId,
                Timestamp = messageRow.Timestamp,
                Content = messageRow.Content.Value
            });
        }

        private async Task MigrateMessgePinningAsync()
        {
            var rows = await MessagePinning.GetRowsAsync();
            _db.MessagePinningConfigurations.RemoveRange(await _db.MessagePinningConfigurations.ToListAsync());
            _db.MessagePinningWebhooks.RemoveRange(await _db.MessagePinningWebhooks.ToListAsync());

            foreach (var row in rows)
            {
                var messagePinningConfiguration = new MessagePinningConfiguration(row.GuildId)
                {
                    PinChannelId = row.PinChannelId,
                    PinMessages = row.Pin
                };
                
                _db.MessagePinningConfigurations.Add(messagePinningConfiguration);
                _logger.LogDebug("Migrated message pinning configuration {GuildId}", row.GuildId);
                
                SetHasFeature(row.GuildId, BotFeatures.MessagePinning, true);
                
                foreach (var rowWebhookId in row.WebhookIds)
                {
                    var messagePinningWebhook = new MessagePinningWebhook(row.GuildId, rowWebhookId.Item1)
                    {
                        WebhookId = rowWebhookId.Item2
                    };

                    _db.MessagePinningWebhooks.Add(messagePinningWebhook);
                    _logger.LogDebug("Migrated message pinning webhook {GuildId}/{ChannelId}", messagePinningWebhook.GuildId, messagePinningWebhook.ChannelId);
                }
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateNoticesAsync()
        {
            var rows = await Notices.GetRowsAsync();
            _db.NoticeConfigurations.RemoveRange(await _db.NoticeConfigurations.ToListAsync());

            foreach (var row in rows)
            {
                var noticeConfiguration = new NoticeConfiguration(row.GuildId, row.ChannelId)
                {
                    Enabled = row.Enabled,
                    Delay = row.Delay,
                    Title = row.Title.Value,
                    Footer = row.Footer.Value,
                    Content = row.Content.Value,
                    Text = row.Text.Value,
                    Image = row.Image.Value,
                    Thumbnail = row.Thumbnail.Value,
                    Icon = row.Icon.Value,
                    Colour = row.Colour,
                    MessageId = row.MessageId,
                    UpdatedFromDashboard = false
                };

                _db.NoticeConfigurations.Add(noticeConfiguration);
                _logger.LogDebug("Migrated notice configuration {GuildId}/{ChannelId}", row.GuildId, row.ChannelId);
                
                SetHasFeature(row.GuildId, BotFeatures.Notices, true);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigratePremiumSlotsAsync()
        {
            var rows = await Premium.GetRowsAsync();
            _db.PremiumSlots.RemoveRange(await _db.PremiumSlots.ToListAsync());

            foreach (var row in rows)
            {
                var premiumSlot = new PremiumSlot(row.UserId)
                {
                    GuildId = row.GuildId
                };

                _db.PremiumSlots.Add(premiumSlot);
                _logger.LogDebug("Mirgated premium slot {SlotId}", row.SlotId);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateReputationAsync()
        {
            var rows = await Reputation.GetRowsAsync();
            _db.ReputationConfigurations.RemoveRange(await _db.ReputationConfigurations.Include(x => x.Emojis).ToListAsync());

            foreach (var row in rows)
            {
                var reputationConfiguration = new ReputationConfiguration(row.GuildId)
                {
                    Emojis = row.Emotes.Select(x => new ReputationConfigurationEmoji(x.Item1)
                    {
                        Value = x.Item2
                    }).ToList()
                };
                
                _db.ReputationConfigurations.Add(reputationConfiguration);
                _logger.LogDebug("Migrated reputation configuration {GuildId}", row.GuildId);
                
                SetHasFeature(row.GuildId, BotFeatures.Reputation, reputationConfiguration.Emojis.Any());
            }

            var memberRows = await Reputation.GetUserRowsAsync();
            _db.ReputationMembers.RemoveRange(await _db.ReputationMembers.ToListAsync());

            foreach (var memberRow in memberRows)
            {
                var reputationMember = new ReputationMember(memberRow.GuildId, memberRow.UserId)
                {
                    Reputation = memberRow.Reputation
                };

                _db.ReputationMembers.Add(reputationMember);
                _logger.LogDebug("Migrated reputation member {GuildId}/{MemberId}", memberRow.GuildId, memberRow.UserId);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateRoleLinkingAsync()
        {
            static RoleLinkingMode GetMode(int mode)
            {
                return mode switch
                {
                    0 => RoleLinkingMode.GrantOnGrant,
                    1 => RoleLinkingMode.RevokeOnGrant,
                    2 => RoleLinkingMode.GrantOnRevoke,
                    3 => RoleLinkingMode.RevokeOnRevoke,
                    _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
                };
            }
            
            var rows = await RoleLinking.GetRowsAsync();
            _db.RoleLinkingConfigurations.RemoveRange(await _db.RoleLinkingConfigurations.ToListAsync());

            foreach (var row in rows)
            {
                var roleLinkingConfiguration = new RoleLinkingConfiguration(row.GuildId)
                {
                    RoleId = row.RoleId,
                    LinkedRoleId = row.LinkedRoleId,
                    Mode = GetMode(row.Mode)
                };

                _db.RoleLinkingConfigurations.Add(roleLinkingConfiguration);
                _logger.LogDebug("Migrated role linking configuration {GuildId}/{RoleId} ({LinkId})", row.GuildId, row.RoleId, row.LinkId);
                
                SetHasFeature(row.GuildId, BotFeatures.RoleLinking, true);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateRolePersistAsync()
        {
            var rows = await RolePersist.GetRowsAsync();
            _db.RolePersistConfigurations.RemoveRange(await _db.RolePersistConfigurations.ToListAsync());

            foreach (var row in rows)
            {
                var rolePersistConfiguration = new RolePersistConfiguration(row.GuildId)
                {
                    Enabled = row.Enabled,
                    ExcludedRoles = row.ExcludedRoles
                };

                _db.RolePersistConfigurations.Add(rolePersistConfiguration);
                _logger.LogDebug("Migrated role persist configuration {GuildId}", row.GuildId);
                
                SetHasFeature(row.GuildId, BotFeatures.RolePersist, rolePersistConfiguration.Enabled);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateSubscriptionsAsync()
        {
            var rows = await Subscriptions.GetRowsAsync();
            _db.Subscriptions.RemoveRange(await _db.Subscriptions.ToListAsync());

            foreach (var row in rows)
            {
                var subscription = new Subscription(row.SubscriptionId)
                {
                    UserId = row.UserId,
                    Slots = row.Slots,
                    Status = (NewDatabase.Entities.SubscriptionStatus) (int) row.Status,
                    ExpiresAt = row.EndsAt,
                };

                _db.Subscriptions.Add(subscription);
                _logger.LogDebug("Migrated subscription {Id}", subscription.Id);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateUsersAsync()
        {
            var rows = await Users.GetRowsAsync();
            _db.Users.RemoveRange(await _db.Users.ToListAsync());
            _db.CustomerDetails.RemoveRange(await _db.CustomerDetails.ToListAsync());

            foreach (var row in rows)
            {
                var user = new User(row.UserId)
                {
                    Email = row.Email,
                };

                if (!string.IsNullOrEmpty(row.CustomerId))
                {
                    var customerDetails = new CustomerDetails(row.CustomerId)
                    {
                        UserId = row.UserId
                    };
                    _db.CustomerDetails.Add(customerDetails);
                }
                
                _db.Users.Add(user);
                _logger.LogDebug("Migrated user {UserId}", row.UserId);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateVoiceLinkAsync()
        {
            var rows = await VoiceLink.GetRowsAsync();
            _db.VoiceLinkConfigurations.RemoveRange(await _db.VoiceLinkConfigurations.ToListAsync());

            foreach (var row in rows)
            {
                var voiceLinkConfiguration = new VoiceLinkConfiguration(row.GuildId)
                {
                    Enabled = row.Enabled,
                    DeleteChannels = row.DeleteChannels,
                    ChannelPrefix = row.Prefix.Value,
                    ExcludedChannels = row.ExcludedChannels
                };

                _db.VoiceLinkConfigurations.Add(voiceLinkConfiguration);
                _logger.LogDebug("Migrated voice link configuration {GuildId}", row.GuildId);
                
                SetHasFeature(row.GuildId, BotFeatures.VoiceLink, voiceLinkConfiguration.Enabled);
            }

            var channelRows = await VoiceLink.GetChannelRowsAsync();
            _db.VoiceLinkChannels.RemoveRange(await _db.VoiceLinkChannels.ToListAsync());

            foreach (var channelRow in channelRows)
            {
                var voiceLinkChannel = new VoiceLinkChannel(channelRow.GuildId, channelRow.VoiceChannelId)
                {
                    TextChannelId = channelRow.TextChannelId
                };

                _db.VoiceLinkChannels.Add(voiceLinkChannel);
                _logger.LogDebug("Migrated voice link channel {GuildId}/{ChannelId}", channelRow.GuildId, channelRow.VoiceChannelId);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateVoiceRolesAsync()
        {
            var rows = await VoiceRoles.GetRowsAsync();
            _db.VoiceRoleConfigurations.RemoveRange(await _db.VoiceRoleConfigurations.ToListAsync());

            foreach (var row in rows)
            {
                var voiceRoleConfiguration = new VoiceRoleConfiguration(row.GuildId, row.ChannelId)
                {
                    RoleId = row.RoleId
                };

                _db.VoiceRoleConfigurations.Add(voiceRoleConfiguration);
                _logger.LogDebug("Migrated voice role configuration {GuildId}/{ChannelId}", row.GuildId, row.ChannelId);
                
                SetHasFeature(row.GuildId, BotFeatures.VoiceRoles, true);
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateVoteChannelsAsync()
        {
            static VoteChannelMode GetMode(int mode)
            {
                return mode switch
                {
                    0 => VoteChannelMode.All,
                    1 => VoteChannelMode.Images,
                    2 => VoteChannelMode.Videos,
                    3 => VoteChannelMode.Images | VoteChannelMode.Videos,
                    4 => VoteChannelMode.Music,
                    5 => VoteChannelMode.Attachments,
                    6 => VoteChannelMode.Links,
                    7 => VoteChannelMode.Images | VoteChannelMode.Videos | VoteChannelMode.Links,
                    8 => VoteChannelMode.Embeds,
                    _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Bad message filter mode")
                };
            }
            
            var rows = await VoteChannels.GetRowsAsync();
            _db.VoteChannelConfigurations.RemoveRange(await _db.VoteChannelConfigurations.ToListAsync());

            foreach (var row in rows)
            {
                var voteChannelConfiguration = new VoteChannelConfiguration(row.GuildId, row.ChannelId)
                {
                    Mode = GetMode(row.Mode),
                    Emojis = row.Emotes
                };

                _db.VoteChannelConfigurations.Add(voteChannelConfiguration);
                _logger.LogDebug("Migrated vote channel configuration {GuildId}/{ChannelId}", row.GuildId, row.ChannelId);
                
                SetHasFeature(row.GuildId, BotFeatures.VoteChannels, true);
            }

            await _db.SaveChangesAsync();
        }
    }
}
