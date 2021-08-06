using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Http;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewDatabase.Entities;
using NewDatabase.Extensions;
using Utili.Extensions;

namespace Utili.Services
{
    public class AutopurgeService
    {
        private readonly ILogger<AutopurgeService> _logger;
        private readonly DiscordClientBase _client;

        private int _purgeNumber;
        private Timer _timer;
        private List<ulong> _downloadingFor = new();

        public AutopurgeService(ILogger<AutopurgeService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;

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
                var rows = await SelectRowsToPurgeAsync();
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

        private async Task<List<AutopurgeRow>> SelectRowsToPurgeAsync()
        {
            var rows = await Autopurge.GetRowsAsync(enabledOnly: true);
            var premium = await Premium.GetRowsAsync();
            var premiumRows = rows.Where(x => premium.Any(y => y.GuildId == x.GuildId)).ToList();
            rows.RemoveAll(x => premium.Any(y => y.GuildId == x.GuildId));

            var rowsToPurge = new List<AutopurgeRow>();

            if (_purgeNumber % 3 == 0)
            {
                for (var i = 0; i < 1; i++)
                {
                    foreach (var guild in _client.GetGuilds().Values)
                    {
                        var guildRows = rows.Where(x => x.GuildId == guild.Id && guild.GetTextChannel(x.ChannelId) is not null).OrderBy(x => x.ChannelId).ToList();
                        if (guildRows.Count > 0)
                        {
                            var row = guildRows[(_purgeNumber / 3) % guildRows.Count];
                            if(rowsToPurge.All(x => x.ChannelId != row.ChannelId)) rowsToPurge.Add(row);
                        }
                    }
                }
            }

            rowsToPurge.AddRange(premiumRows);

            _purgeNumber++;
            if (_purgeNumber == int.MaxValue) _purgeNumber = 0;

            return rowsToPurge;
        }

        private async Task PurgeChannelAsync(AutopurgeRow row)
        {
            try
            {
                row = await Autopurge.GetRowAsync(row.GuildId, row.ChannelId);
                if(row.Mode == 2) return;

                var guild = _client.GetGuild(row.GuildId);
                var channel = guild.GetTextChannel(row.ChannelId);

                if(!channel.BotHasPermissions(Permission.ViewChannel | Permission.ReadMessageHistory | Permission.ManageMessages)) return;

                var messagesToDelete = await Autopurge.GetAndDeleteDueMessagesAsync(row);
                if(messagesToDelete.Count == 0) return;

                await channel.DeleteMessagesAsync(messagesToDelete.Select(x => new Snowflake(x.MessageId)), new DefaultRestRequestOptions {Reason = "Autopurge"});
            }
            catch (RestApiException ex) when (ex.StatusCode == HttpResponseStatusCode.NotFound)
            {
                _logger.LogDebug(ex, $"Exception thrown while purging channel {row.GuildId}/{row.ChannelId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception thrown while purging channel {row.GuildId}/{row.ChannelId}");
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = PurgeChannelsAsync();
            _ = FetchForNewChannelsAsync();
            _ = Autopurge.DeleteOldMessagesAsync();
        }

        public async Task MessageReceived(IServiceScope scope, MessageReceivedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue) return;

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

        public async Task MessageUpdated(MessageUpdatedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue || !e.Model.Pinned.HasValue) return;
                var row = await Autopurge.GetRowAsync(e.GuildId.Value, e.ChannelId);
                if (row.Mode == 2) return;

                var messageRow = (await Autopurge.GetMessagesAsync(e.GuildId.Value, e.ChannelId, e.MessageId)).FirstOrDefault();
                if (messageRow is null) return;
                if (messageRow.IsPinned != e.Model.Pinned.Value)
                {
                    messageRow.IsPinned = e.Model.Pinned.Value;
                    await Autopurge.SaveMessageAsync(messageRow);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message updated");
            }
        }

        public async Task MessageDeleted(MessageDeletedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue) return;
                var row = await Autopurge.GetRowAsync(e.GuildId.Value, e.ChannelId);
                if (row.Mode == 2) return;

                var messageRow =
                    (await Autopurge.GetMessagesAsync(e.GuildId.Value, e.ChannelId, e.MessageId)).FirstOrDefault();
                if (messageRow is null) return;
                
                await Autopurge.DeleteMessageAsync(messageRow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message deleted");
            }
        }
        
        public async Task MessagesDeleted(MessagesDeletedEventArgs e)
        {
            try
            {
                var row = await Autopurge.GetRowAsync(e.GuildId, e.ChannelId);
                if (row.Mode == 2) return;

                await Autopurge.DeleteMessagesAsync(row, e.MessageIds.Select(x => x.RawValue).ToArray());
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
                var rows = await Autopurge.GetRowsAsync(enabledOnly: true);
                rows.RemoveAll(x => _client.GetGuild(x.GuildId)?.GetTextChannel(x.ChannelId) is null);

                _logger.LogInformation($"Started downloading messages for {rows.Count} channels");

                var tasks = new List<Task>();
                foreach (var row in rows)
                {
                    while (tasks.Count(x => !x.IsCompleted) >= 10)
                        await Task.Delay(1000);

                    tasks.Add(FetchForChannelAsync(row));
                    await Task.Delay(250);
                }

                await Task.WhenAll(tasks);

                _logger.LogInformation("Finished downloading messages");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on while fetching messages for all channels");
            }
        }

        private async Task FetchForNewChannelsAsync()
        {
            try
            {
                var miscRows = await Misc.GetRowsAsync(type: "RequiresAutopurgeMessageDownload");
                miscRows.RemoveAll(x => _client.GetGuild(x.GuildId) is null);

                foreach (var miscRow in miscRows)
                    _ = Misc.DeleteRowAsync(miscRow);

                foreach (var miscRow in miscRows)
                {
                    var row = await Autopurge.GetRowAsync(miscRow.GuildId, ulong.Parse(miscRow.Value));
                    _ = FetchForChannelAsync(row);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while fetching messages for new channels");
            }
        }

        private Task FetchForChannelAsync(AutopurgeRow row)
        {
            return Task.Run(async () =>
            {
                lock (_downloadingFor)
                {
                    if (_downloadingFor.Contains(row.ChannelId)) return;
                    _downloadingFor.Add(row.ChannelId);
                }

                try
                {
                    var guild = _client.GetGuild(row.GuildId);
                    var channel = guild.GetTextChannel(row.ChannelId);

                    if(!channel.BotHasPermissions(Permission.ViewChannel | Permission.ReadMessageHistory)) return;

                    var messageRows =
                        await Autopurge.GetMessagesAsync(guild.Id, channel.Id);

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

                    foreach (var message in messages)
                    {
                        var messageRow = messageRows.FirstOrDefault(x => x.MessageId == message.Id);
                        if (messageRow is not null)
                        {
                            var pinned = message is IUserMessage {IsPinned: true};
                            if (messageRow.IsPinned != pinned)
                            {
                                messageRow.IsPinned = pinned;
                                await Autopurge.SaveMessageAsync(messageRow);
                            }
                        }
                        else
                        {
                            messageRow = new AutopurgeMessageRow
                            {
                                GuildId = guild.Id,
                                ChannelId = channel.Id,
                                MessageId = message.Id,
                                Timestamp = message.CreatedAt().UtcDateTime,
                                IsBot = message.Author.IsBot,
                                IsPinned = message is IUserMessage {IsPinned: true}
                            };
                            try { await Autopurge.SaveMessageAsync(messageRow); } catch { }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Exception thrown while fetching messages for channel {row.GuildId}/{row.ChannelId}");
                }
                finally
                {
                    lock (_downloadingFor)
                    {
                        _downloadingFor.Remove(row.ChannelId);
                    }
                }
            });
        }
    }
}
