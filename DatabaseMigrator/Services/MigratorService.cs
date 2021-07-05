using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewDatabase;
using Database.Data;
using Microsoft.EntityFrameworkCore;
using NewDatabase.Entities;

namespace DatabaseMigrator.Services
{
    
    public class MigratorService
    {
        private static readonly bool UseProdDb = true;
        
        private readonly ILogger<MigratorService> _logger;
        private readonly DatabaseContext _db;

        public MigratorService(ILogger<MigratorService> logger, DatabaseContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task RunAsync()
        {
            try
            {
                await Database.Database.InitialiseAsync(false, ".", UseProdDb);
                _logger.LogInformation("Old database initialised - Prod: {Prod}", UseProdDb);

                _logger.LogInformation("Migrating autopurge...");
                await MigrateAutopurgeAsync();
                
                _logger.LogInformation("Migrating core...");
                await MigrateCoreAsync();
                
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
                
                _logger.LogInformation("Finished");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while running");
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
            }

            /*
            var messageRows = await Autopurge.GetMessagesAsync();
            _db.AutopurgeMessages.RemoveRange(await _db.AutopurgeMessages.ToListAsync());
            
            foreach (var messageRow in messageRows.Where(x => !messageKeys.Contains(x.MessageId)))
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
            }
            */

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
                    Mode = row.Inverse ? InactiveRoleMode.GrantWhenInactive : InactiveRoleMode.RevokeWhenInactive,
                    AutoKick = row.AutoKick,
                    AutoKickThreshold = row.AutoKickThreshold,
                    DefaultLastAction = row.DefaultLastAction,
                    LastUpdate = row.LastUpdate
                };
                _db.InactiveRoleConfigurations.Add(inactiveRoleConfiguration);
                _logger.LogDebug("Migrated inactive role configuration {GuildId}", row.GuildId);
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
            }

            var messageRows = await MessageLogs.GetMessagesAsync();
            _db.MessageLogsMessages.RemoveRange(await _db.MessageLogsMessages.ToListAsync());

            foreach (var messageRow in messageRows)
            {
                var messageLogsMessage = new MessageLogsMessage(messageRow.MessageId)
                {
                    GuildId = messageRow.GuildId,
                    ChannelId = messageRow.ChannelId,
                    AuthorId = messageRow.UserId,
                    Timestamp = messageRow.Timestamp,
                    Content = messageRow.Content.Value
                };

                _db.MessageLogsMessages.Add(messageLogsMessage);
                _logger.LogDebug("Migrated message logs message {MessageId}", messageRow.MessageId);
            }

            await _db.SaveChangesAsync();
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
            _db.ReputationConfigurations.RemoveRange(await _db.ReputationConfigurations.ToListAsync());

            foreach (var row in rows)
            {
                var reputationConfiguration = new ReputationConfiguration(row.GuildId)
                {
                    Emojis = row.Emotes.Select(x => new ReputationConfigurationEmoji(row.GuildId, x.Item1)
                    {
                        Value = x.Item2
                    }).ToList()
                };

                _db.ReputationConfigurations.Add(reputationConfiguration);
                _logger.LogDebug("Migrated reputation configuration {GuildId}", row.GuildId);
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
                var roleLinkingConfiguration = new RoleLinkingConfiguration(row.GuildId, row.RoleId)
                {
                    LinkedRoleId = row.LinkedRoleId,
                    Mode = GetMode(row.Mode)
                };

                _db.RoleLinkingConfigurations.Add(roleLinkingConfiguration);
                _logger.LogDebug("Migrated role linking configuration {GuildId}/{RoleId} ({LinkId})", row.GuildId, row.RoleId, row.LinkId);
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
            }

            await _db.SaveChangesAsync();
        }

        private async Task MigrateSubscriptionsAsync()
        {
            var rows = await Subscriptions.GetRowsAsync();
            _db.Subscriptions.RemoveRange(await _db.Subscriptions.ToListAsync());

            foreach (var row in rows)
            {
                var subscription = new Subscription(row.SubscriptionId, row.UserId, row.Slots)
                {
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

            foreach (var row in rows)
            {
                var user = new User(row.UserId)
                {
                    Email = row.Email,
                    CustomerId = row.CustomerId
                };

                _db.Users.Add(user);
                _logger.LogDebug("Migrated user {UserId}", row.UserId);
            }

            await _db.SaveChangesAsync();
        }
    }
}
