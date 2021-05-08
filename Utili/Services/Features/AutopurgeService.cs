using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class AutopurgeService
    {
        ILogger<AutopurgeService> _logger;
        DiscordClientBase _client;

        int _purgeNumber;
        Timer _timer;
        List<ulong> _downloadingFor = new List<ulong>();
        

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
            _ = GetMessagesAsync();
        }

        async Task PurgeChannelsAsync()
        {
            try
            {
                List<AutopurgeRow> rows = await SelectRowsToPurgeAsync();
                rows.RemoveAll(x => _client.GetGuild(x.GuildId) is null);
                rows.RemoveAll(x => _client.GetGuild(x.GuildId).GetTextChannel(x.ChannelId) is null);

                List<Task> tasks = new List<Task>();
                foreach (AutopurgeRow row in rows)
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

        async Task<List<AutopurgeRow>> SelectRowsToPurgeAsync()
        {
            List<AutopurgeRow> rows = await Autopurge.GetRowsAsync(enabledOnly: true);
            List<PremiumRow> premium = await Premium.GetRowsAsync();
            List<AutopurgeRow> premiumRows = rows.Where(x => premium.Any(y => y.GuildId == x.GuildId)).ToList();
            rows.RemoveAll(x => premium.Any(y => y.GuildId == x.GuildId));

            List<AutopurgeRow> rowsToPurge = new List<AutopurgeRow>();

            if (_purgeNumber % 3 == 0)
            {
                for (int i = 0; i < 1; i++)
                {
                    foreach (CachedGuild guild in _client.GetGuilds().Values)
                    {
                        List<AutopurgeRow> guildRows = rows.Where(x => x.GuildId == guild.Id && guild.GetTextChannel(x.ChannelId) is not null).OrderBy(x => x.ChannelId).ToList();
                        if (guildRows.Count > 0)
                        {
                            AutopurgeRow row = guildRows[(_purgeNumber / 3) % guildRows.Count];
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

        async Task PurgeChannelAsync(AutopurgeRow row)
        {
            try
            {
                row = await Autopurge.GetRowAsync(row.GuildId, row.ChannelId);
                if(row.Mode == 2) return;

                CachedGuild guild = _client.GetGuild(row.GuildId);
                CachedTextChannel channel = guild.GetTextChannel(row.ChannelId);

                if(!channel.BotHasPermissions(Permission.ViewChannel | Permission.ReadMessageHistory | Permission.ManageMessages)) return;

                List<AutopurgeMessageRow> messagesToDelete = await Autopurge.GetAndDeleteDueMessagesAsync(row);
                if(messagesToDelete.Count == 0) return;

                await channel.DeleteMessagesAsync(messagesToDelete.Select(x => new Snowflake(x.MessageId)));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception thrown while purging channel {row.GuildId}/{row.ChannelId}");
            }
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = PurgeChannelsAsync();
            _ = GetNewChannelsMessagesAsync();
        }

        public async Task MessageReceived(MessageReceivedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue) return;

                AutopurgeRow row = await Autopurge.GetRowAsync(e.GuildId.Value, e.ChannelId);
                if (row.Mode == 2) return;

                AutopurgeMessageRow messageRow = new AutopurgeMessageRow
                {
                    GuildId = e.GuildId.Value,
                    ChannelId = e.ChannelId,
                    MessageId = e.MessageId,
                    Timestamp = e.Message.CreatedAt.UtcDateTime,
                    IsBot = e.Message.Author.IsBot,
                    IsPinned = e.Message is IUserMessage userMessage && userMessage.IsPinned
                };
                await Autopurge.SaveMessageAsync(messageRow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received");
            }
        }

        public async Task MessageUpdated(MessageUpdatedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue) return;
                AutopurgeRow row = await Autopurge.GetRowAsync(e.GuildId.Value, e.ChannelId);
                if (row.Mode == 2) return;

                AutopurgeMessageRow messageRow =
                    (await Autopurge.GetMessagesAsync(e.GuildId.Value, e.ChannelId, e.MessageId)).FirstOrDefault();
                if (messageRow is null)
                {
                    messageRow = new AutopurgeMessageRow
                    {
                        GuildId = e.GuildId.Value,
                        ChannelId = e.ChannelId,
                        MessageId = e.MessageId,
                        Timestamp = e.NewMessage.CreatedAt.UtcDateTime,
                        IsBot = e.NewMessage.Author.IsBot,
                        IsPinned = e.NewMessage.IsPinned
                    };
                    await Autopurge.SaveMessageAsync(messageRow);
                    return;
                }

                if (messageRow.IsPinned != e.NewMessage.IsPinned)
                {
                    messageRow.IsPinned = e.NewMessage.IsPinned;
                    await Autopurge.SaveMessageAsync(messageRow);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message updated");
            }
        }

        async Task GetMessagesAsync()
        {
            List<AutopurgeRow> rows = await Autopurge.GetRowsAsync(enabledOnly: true);
            rows.RemoveAll(x => _client.GetGuild(x.GuildId) is null);
            rows.RemoveAll(x => _client.GetGuild(x.GuildId).GetTextChannel(x.ChannelId) is null);

            _logger.LogInformation($"Started downloading messages for {rows.Count} channels");

            List<Task> tasks = new List<Task>();
            foreach (AutopurgeRow row in rows)
            {
                while (tasks.Count(x => !x.IsCompleted) >= 10) 
                    await Task.Delay(1000);

                tasks.Add(GetChannelMessagesAsync(row));
                await Task.Delay(250);
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Finished downloading messages");
        }

        async Task GetNewChannelsMessagesAsync()
        {
            _ = Task.Run(async () =>
            {
                List<MiscRow> miscRows = await Misc.GetRowsAsync(type: "RequiresAutopurgeMessageDownload");
                miscRows.RemoveAll(x => _client.GetGuild(x.GuildId) is null);

                foreach (MiscRow miscRow in miscRows)
                    _ = Misc.DeleteRowAsync(miscRow);

                foreach (MiscRow miscRow in miscRows)
                {
                    AutopurgeRow row = await Autopurge.GetRowAsync(miscRow.GuildId, ulong.Parse(miscRow.Value));
                    _ = GetChannelMessagesAsync(row);
                }
            });
        }

        Task GetChannelMessagesAsync(AutopurgeRow row)
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
                    CachedGuild guild = _client.GetGuild(row.GuildId);
                    CachedTextChannel channel = guild.GetTextChannel(row.ChannelId);

                    if(!channel.BotHasPermissions(Permission.ViewChannel | Permission.ReadMessageHistory)) return;

                    List<AutopurgeMessageRow> messageRows =
                        await Autopurge.GetMessagesAsync(guild.Id, channel.Id);

                    List<IMessage> messages = new List<IMessage>();
                    IMessage oldestMessage = null;

                    while (true)
                    {
                        List<IMessage> fetchedMessages;
                        if (oldestMessage is null)
                            fetchedMessages = (await channel.FetchMessagesAsync()).ToList();
                        else
                            fetchedMessages = (await channel.FetchMessagesAsync(100, RetrievalDirection.Before, oldestMessage.Id)).ToList();

                        if (fetchedMessages.Count == 0) break;
                        oldestMessage = fetchedMessages.OrderBy(x => x.CreatedAt.UtcDateTime).First();

                        messages.AddRange(fetchedMessages.Where(x =>
                            x.CreatedAt.UtcDateTime > DateTime.UtcNow.AddDays(-13.9)));

                        if (messages.Count < 100 ||
                            oldestMessage.CreatedAt.UtcDateTime < DateTime.UtcNow.AddDays(-13.9)) break;

                        await Task.Delay(1000);
                    }

                    foreach (IMessage message in messages)
                    {
                        AutopurgeMessageRow messageRow = messageRows.FirstOrDefault(x => x.MessageId == message.Id);
                        if (messageRow is not null)
                        {
                            bool pinned = message is IUserMessage userMessage && userMessage.IsPinned;
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
                                Timestamp = message.CreatedAt.UtcDateTime,
                                IsBot = message.Author.IsBot,
                                IsPinned = message is IUserMessage userMessage && userMessage.IsPinned
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
