using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Discord;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Features
{
    internal static class Autopurge
    {
        static Timer _timer;

        public static void Start()
        {
            _timer = new Timer(5000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
            _ = GetMessagesAsync();
        }

        private static async Task PurgeChannelsAsync()
        {
            List<AutopurgeRow> rows = await Database.Data.Autopurge.GetRowsAsync(enabledOnly: true);
            rows.RemoveAll(x => _client.Guilds.All(y => y.Id != x.GuildId));
            rows.RemoveAll(x => _client.GetGuild(x.GuildId).TextChannels.All(y => y.Id != x.ChannelId));

            List<Task> tasks = new List<Task>();
            foreach (AutopurgeRow row in rows)
            {
                tasks.Add(PurgeChannelAsync(row));
                await Task.Delay(250);
            }

            await Task.WhenAll(tasks);
        }

        private static async Task PurgeChannelAsync(AutopurgeRow row)
        {
            try
            {
                SocketGuild guild = _client.GetGuild(row.GuildId);
                SocketTextChannel channel = guild.GetTextChannel(row.ChannelId);

                List<AutopurgeMessageRow> messagesToDelete = await Database.Data.Autopurge.GetAndDeleteDueMessagesAsync(row);
                if(messagesToDelete.Count == 0) return;

                await channel.DeleteMessagesAsync(messagesToDelete.Select(x => x.MessageId), new RequestOptions { RetryMode = RetryMode.AlwaysRetry });
            }
            catch (Exception e)
            {
                _logger.ReportError("AutopurgeCnl", e);
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = PurgeChannelsAsync();
        }

        public static async Task MessageReceived(SocketMessage message)
        {
            SocketTextChannel channel = message.Channel as SocketTextChannel;
            SocketGuild guild = channel.Guild;

            AutopurgeRow row = await Database.Data.Autopurge.GetRowAsync(guild.Id, channel.Id);
            if(row.Mode == 2) return;

            AutopurgeMessageRow messageRow = new AutopurgeMessageRow
            {
                GuildId = guild.Id,
                ChannelId = channel.Id,
                MessageId = message.Id,
                Timestamp = message.Timestamp.UtcDateTime,
                IsBot = message.Author.IsBot,
                IsPinned = message.IsPinned
            };
            await Database.Data.Autopurge.SaveMessageAsync(messageRow);
        }

        public static async Task MessageEdited(SocketMessage message)
        {
            SocketTextChannel channel = message.Channel as SocketTextChannel;
            SocketGuild guild = channel.Guild;

            AutopurgeRow row = await Database.Data.Autopurge.GetRowAsync(guild.Id, channel.Id);
            if(row.Mode == 2) return;

            AutopurgeMessageRow messageRow = (await Database.Data.Autopurge.GetMessagesAsync(guild.Id, channel.Id, message.Id)).FirstOrDefault();
            if(messageRow is null) return;

            if (messageRow.IsPinned != message.IsPinned)
            {
                messageRow.IsPinned = message.IsPinned;
                await Database.Data.Autopurge.SaveMessageAsync(messageRow);
            }
        }

        private static async Task GetMessagesAsync()
        {
            List<AutopurgeRow> rows = await Database.Data.Autopurge.GetRowsAsync(enabledOnly: true);
            rows.RemoveAll(x => _client.Guilds.All(y => y.Id != x.GuildId));
            rows.RemoveAll(x => _client.GetGuild(x.GuildId).TextChannels.All(y => y.Id != x.ChannelId));

            List<Task> tasks = new List<Task>();
            foreach (AutopurgeRow row in rows)
            {
                tasks.Add(GetChannelMessagesAsync(row));
                await Task.Delay(500);
            }

            await Task.WhenAll(tasks);
        }

        private static async Task GetChannelMessagesAsync(AutopurgeRow row)
        {
            try
            {
                SocketGuild guild = _client.GetGuild(row.GuildId);
                SocketTextChannel channel = guild.GetTextChannel(row.ChannelId);

                List<AutopurgeMessageRow> messageRows = await Database.Data.Autopurge.GetMessagesAsync(guild.Id, channel.Id);

                List<IMessage> messages = new List<IMessage>();
                IMessage oldestMessage = null;

                while (true)
                {
                    List<IMessage> fetchedMessages;
                    if (oldestMessage is null)
                        fetchedMessages = (await channel.GetMessagesAsync().FlattenAsync()).ToList();
                    else
                        fetchedMessages = (await channel.GetMessagesAsync(oldestMessage, Direction.Before).FlattenAsync()).ToList();

                    if(fetchedMessages.Count == 0) break;
                    oldestMessage = fetchedMessages.OrderBy(x => x.Timestamp.UtcDateTime).First();

                    messages.AddRange(fetchedMessages.Where(x => x.Timestamp.UtcDateTime > DateTime.UtcNow.AddDays(-13.9)));

                    if(messages.Count < 100 || oldestMessage.Timestamp.UtcDateTime < DateTime.UtcNow.AddDays(-13.9)) break;

                    await Task.Delay(1000);
                }
                
                foreach(IMessage message in messages)
                {
                    AutopurgeMessageRow messageRow = messageRows.FirstOrDefault(x => x.MessageId == message.Id);
                    if(messageRow is not null)
                    {
                        if (messageRow.IsPinned != message.IsPinned)
                        {
                            messageRow.IsPinned = message.IsPinned;
                            await Database.Data.Autopurge.SaveMessageAsync(messageRow);
                        }
                    }
                    else
                    {
                        messageRow = new AutopurgeMessageRow
                        {
                            GuildId = guild.Id,
                            ChannelId = channel.Id,
                            MessageId = message.Id,
                            Timestamp = message.Timestamp.UtcDateTime,
                            IsBot = message.Author.IsBot,
                            IsPinned = message.IsPinned
                        };
                        await Database.Data.Autopurge.SaveMessageAsync(messageRow);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.ReportError("AutopurgeGet", e);
            }
        }
    }
}
