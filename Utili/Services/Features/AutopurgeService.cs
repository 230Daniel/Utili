using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Disqord;
using Disqord.Gateway;
using Disqord.Http;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Database.Entities;
using Database.Extensions;
using Utili.Extensions;

namespace Utili.Services
{
    public class AutopurgeService
    {
        private readonly ILogger<AutopurgeService> _logger;
        private readonly DiscordClientBase _client;
        private readonly IServiceScopeFactory _scopeFactory;

        private int _purgeNumber;
        private Timer _timer;
        private List<ulong> _downloadingFor = new();

        public AutopurgeService(ILogger<AutopurgeService> logger, DiscordClientBase client, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _client = client;
            _scopeFactory = scopeFactory;

            _purgeNumber = 0;
            _timer = new Timer(10000);
            _timer.Elapsed += Timer_Elapsed;
        }

        public void Start()
        {
            _timer.Start();
            _ = FetchForAllChannelsAsync();
        }

        private async Task PurgeChannelsAsync()
        {
            try
            {
                var rows = await SelectChannelsToPurgeAsync();
                rows.RemoveAll(x => _client.GetGuild(x.GuildId) is null);
                rows.RemoveAll(x => _client.GetGuild(x.GuildId).GetTextChannel(x.ChannelId) is null);

                var tasks = new List<Task>();
                foreach (var row in rows)
                {
                    tasks.Add(PurgeChannelAsync(row));
                    await Task.Delay(250);
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception thrown purging all channels");
            }
        }

        private async Task<List<AutopurgeConfiguration>> SelectChannelsToPurgeAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.GetDbContext();

            var configs = await db.AutopurgeConfigurations.Where(x => x.Mode != AutopurgeMode.None).ToListAsync();
            var premiumSlots = await db.PremiumSlots.ToListAsync();

            var premiumConfigs = configs.Where(x => premiumSlots.Any(y => y.GuildId == x.GuildId)).ToList();
            configs.RemoveAll(x => premiumSlots.Any(y => y.GuildId == x.GuildId));

            var channelsToPurge = new List<AutopurgeConfiguration>();

            if (_purgeNumber % 3 == 0)
            {
                foreach (var guild in _client.GetGuilds().Values)
                {
                    var guildConfigs = configs
                        .Where(x => x.GuildId == guild.Id && guild.GetTextChannel(x.ChannelId) is not null)
                        .OrderBy(x => x.ChannelId)
                        .ToList();

                    if (guildConfigs.Count > 0)
                    {
                        var config = guildConfigs[(_purgeNumber / 3) % guildConfigs.Count];
                        if(channelsToPurge.All(x => x.ChannelId != config.ChannelId)) channelsToPurge.Add(config);
                    }
                }
            }

            channelsToPurge.AddRange(premiumConfigs);

            _purgeNumber++;
            if (_purgeNumber == int.MaxValue) _purgeNumber = 0;

            return channelsToPurge;
        }

        private async Task PurgeChannelAsync(AutopurgeConfiguration staleConfig)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.GetDbContext();
                var config = await db.AutopurgeConfigurations.GetForGuildChannelAsync(staleConfig.GuildId, staleConfig.ChannelId);
                if(config is null || config.Mode == AutopurgeMode.None || config.Timespan > TimeSpan.FromDays(14)) return;

                var guild = _client.GetGuild(config.GuildId);
                var channel = guild.GetTextChannel(config.ChannelId);
                if(!channel.BotHasPermissions(Permission.ViewChannels | Permission.ReadMessageHistory | Permission.ManageMessages)) return;

                var now = DateTime.UtcNow;
                var maxTimestamp = now - config.Timespan;
                var minTimestamp = now - TimeSpan.FromDays(13.9);

                var query = db.AutopurgeMessages.Where(
                    x => x.GuildId == config.GuildId
                         && x.ChannelId == config.ChannelId
                         && x.Timestamp <= maxTimestamp
                         && x.Timestamp >= minTimestamp
                         && !x.IsPinned);

                var messagesToDelete = config.Mode switch
                {
                    AutopurgeMode.All => await query.ToListAsync(),
                    AutopurgeMode.Bot => await query.Where(x => x.IsBot).ToListAsync(),
                    AutopurgeMode.User => await query.Where(x => !x.IsBot).ToListAsync(),
                    AutopurgeMode.None => new List<AutopurgeMessage>(),
                    _ => throw new Exception($"Unknown autopurge mode {config.Mode}")
                };

                if(messagesToDelete.Count == 0) return;

                db.AutopurgeMessages.RemoveRange(messagesToDelete);
                await db.SaveChangesAsync();

                await channel.DeleteMessagesAsync(messagesToDelete.Select(x => new Snowflake(x.MessageId)), new DefaultRestRequestOptions {Reason = "Autopurge"});
            }
            catch (RestApiException ex) when (ex.StatusCode == HttpResponseStatusCode.NotFound)
            {
                _logger.LogDebug(ex, $"Exception thrown while purging channel {staleConfig.GuildId}/{staleConfig.ChannelId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception thrown while purging channel {staleConfig.GuildId}/{staleConfig.ChannelId}");
            }
        }

        private async Task DeleteOldMessagesAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.GetDbContext();

                var minTimestamp = DateTime.UtcNow - TimeSpan.FromDays(14);
                await db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM autopurge_messages WHERE timestamp < {minTimestamp};");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown deleting old messages");
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = PurgeChannelsAsync();
            _ = FetchForNewChannelsAsync();
            _ = DeleteOldMessagesAsync();
        }

        public async Task MessageReceived(IServiceScope scope, MessageReceivedEventArgs e)
        {
            try
            {
                var db = scope.GetDbContext();
                var config = await db.AutopurgeConfigurations.GetForGuildChannelAsync(e.GuildId.Value, e.ChannelId);
                if (config is null) return;

                var message = new AutopurgeMessage(e.MessageId)
                {
                    GuildId = e.GuildId.Value,
                    ChannelId = e.ChannelId,
                    Timestamp = e.Message.CreatedAt().UtcDateTime,
                    IsBot = e.Message.Author.IsBot,
                    IsPinned = e.Message is IUserMessage {IsPinned: true}
                };

                db.AutopurgeMessages.Add(message);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received ({Guild}/{Channel}/{Message})", e.GuildId, e.ChannelId, e.MessageId);
            }
        }

        public async Task MessageUpdated(IServiceScope scope, MessageUpdatedEventArgs e)
        {
            try
            {
                if (!e.Model.Pinned.HasValue) return;

                var db = scope.GetDbContext();
                var config = await db.AutopurgeConfigurations.GetForGuildChannelAsync(e.GuildId.Value, e.ChannelId);
                if(config is null) return;

                var messageRecord = await db.AutopurgeMessages.GetForMessageAsync(e.MessageId);
                if (messageRecord is null) return;

                if (messageRecord.IsPinned != e.Model.Pinned.Value)
                {
                    messageRecord.IsPinned = e.Model.Pinned.Value;
                    db.AutopurgeMessages.Update(messageRecord);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message updated");
            }
        }

        public async Task MessageDeleted(IServiceScope scope, MessageDeletedEventArgs e)
        {
            try
            {
                var db = scope.GetDbContext();
                var config = await db.AutopurgeConfigurations.GetForGuildChannelAsync(e.GuildId.Value, e.ChannelId);
                if (config is null) return;

                var messageRecord = await db.AutopurgeMessages.GetForMessageAsync(e.MessageId);
                if (messageRecord is null) return;

                db.AutopurgeMessages.Remove(messageRecord);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message deleted");
            }
        }

        public async Task MessagesDeleted(IServiceScope scope, MessagesDeletedEventArgs e)
        {
            try
            {
                var db = scope.GetDbContext();
                var config = await db.AutopurgeConfigurations.GetForGuildChannelAsync(e.GuildId, e.ChannelId);
                if(config is null) return;

                var messages = await db.AutopurgeMessages.Where(x => e.MessageIds.Select(y => y.RawValue).Contains(x.MessageId)).ToListAsync();

                if (messages.Any())
                {
                    db.AutopurgeMessages.RemoveRange(messages);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on messages deleted");
            }
        }

        private async Task FetchForAllChannelsAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.GetDbContext();

                var configs = await db.AutopurgeConfigurations.Where(x => x.Mode != AutopurgeMode.None).ToListAsync();
                configs.RemoveAll(x => _client.GetGuild(x.GuildId)?.GetTextChannel(x.ChannelId) is null);

                _logger.LogInformation($"Started downloading messages for {configs.Count} channels");

                var tasks = new List<Task>();
                foreach (var config in configs)
                {
                    while (tasks.Count(x => !x.IsCompleted) >= 10)
                        await Task.Delay(1000);

                    tasks.Add(FetchForChannelAsync(config));
                    await Task.Delay(250);
                }

                await Task.WhenAll(tasks);

                _logger.LogInformation("Finished downloading messages");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while fetching messages for all channels");
            }
        }

        private async Task FetchForNewChannelsAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.GetDbContext();

                var newConfigs = await db.AutopurgeConfigurations.Where(x => x.AddedFromDashboard).ToListAsync();

                foreach (var newConfig in newConfigs)
                {
                    _ = FetchForChannelAsync(newConfig);
                    newConfig.AddedFromDashboard = false;
                    db.AutopurgeConfigurations.Update(newConfig);
                }

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while fetching messages for new channels");
            }
        }

        private Task FetchForChannelAsync(AutopurgeConfiguration config)
        {
            return Task.Run(async () =>
            {
                lock (_downloadingFor)
                {
                    if (_downloadingFor.Contains(config.ChannelId)) return;
                    _downloadingFor.Add(config.ChannelId);
                }

                try
                {
                    var guild = _client.GetGuild(config.GuildId);
                    var channel = guild.GetTextChannel(config.ChannelId);

                    if(!channel.BotHasPermissions(Permission.ViewChannels | Permission.ReadMessageHistory)) return;

                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.GetDbContext();

                    var messages = new List<IMessage>();
                    IMessage oldestMessage = null;

                    while (true)
                    {
                        List<IMessage> fetchedMessages;
                        if (oldestMessage is null)
                            fetchedMessages = (await channel.FetchMessagesAsync()).ToList();
                        else
                            fetchedMessages = (await channel.FetchMessagesAsync(100, RetrievalDirection.Before, oldestMessage.Id)).ToList();

                        if (fetchedMessages.Count == 0) break;
                        oldestMessage = fetchedMessages.OrderBy(x => x.CreatedAt().UtcDateTime).First();

                        messages.AddRange(fetchedMessages.Where(x =>
                            x.CreatedAt().UtcDateTime > DateTime.UtcNow.AddDays(-14)));

                        if (messages.Count < 100 ||
                            oldestMessage.CreatedAt().UtcDateTime < DateTime.UtcNow.AddDays(-14)) break;

                        await Task.Delay(1000);
                    }

                    var messageRows = await db.AutopurgeMessages
                        .Where(x => x.GuildId == config.GuildId && x.ChannelId == config.ChannelId).ToListAsync();

                    foreach (var message in messages)
                    {
                        var messageRow = messageRows.FirstOrDefault(x => x.MessageId == message.Id);
                        if (messageRow is not null)
                        {
                            var pinned = message is IUserMessage {IsPinned: true};
                            if (messageRow.IsPinned != pinned)
                            {
                                messageRow.IsPinned = pinned;
                                db.AutopurgeMessages.Update(messageRow);
                            }
                        }
                        else
                        {
                            messageRow = new AutopurgeMessage(message.Id)
                            {
                                GuildId = guild.Id,
                                ChannelId = channel.Id,
                                Timestamp = message.CreatedAt().UtcDateTime,
                                IsBot = message.Author.IsBot,
                                IsPinned = message is IUserMessage {IsPinned: true}
                            };
                            db.AutopurgeMessages.Add(messageRow);
                        }
                    }

                    await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception thrown while fetching messages for channel {config.GuildId}/{config.ChannelId}");
                }
                finally
                {
                    lock (_downloadingFor)
                    {
                        _downloadingFor.Remove(config.ChannelId);
                    }
                }
            });
        }
    }
}
