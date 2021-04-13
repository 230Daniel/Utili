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
        static int _purgeNumber;
        static Timer _timer;
        static List<ulong> _downloadingFor = new List<ulong>();

        public static void Start()
        {
            _purgeNumber = 0;
            _timer = new Timer(10000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
            _ = GetMessagesAsync();
        }

        private static async Task PurgeChannelsAsync()
        {
            //List<AutopurgeRow> rows = await Database.Data.Autopurge.GetRowsAsync(enabledOnly: true);

            List<AutopurgeRow> rows = await SelectRowsToPurgeAsync();
            rows.RemoveAll(x => _oldClient.Guilds.All(y => y.Id != x.GuildId));
            rows.RemoveAll(x => _oldClient.GetGuild(x.GuildId).TextChannels.All(y => y.Id != x.ChannelId));

            List<Task> tasks = new List<Task>();
            foreach (AutopurgeRow row in rows)
            {
                tasks.Add(PurgeChannelAsync(row));
                await Task.Delay(250);
            }

            await Task.WhenAll(tasks);
        }

        private static async Task<List<AutopurgeRow>> SelectRowsToPurgeAsync()
        {
            List<AutopurgeRow> rows = await Database.Data.Autopurge.GetRowsAsync(enabledOnly: true);
            List<PremiumRow> premium = await Premium.GetRowsAsync();
            List<AutopurgeRow> premiumRows = rows.Where(x => premium.Any(y => y.GuildId == x.GuildId)).ToList();
            rows.RemoveAll(x => premium.Any(y => y.GuildId == x.GuildId));

            List<AutopurgeRow> rowsToPurge = new List<AutopurgeRow>();

            if (_purgeNumber % 3 == 0)
            {
                for (int i = 0; i < 1; i++)
                {
                    foreach (SocketGuild guild in _oldClient.Guilds)
                    {
                        List<AutopurgeRow> guildRows = rows.Where(x => x.GuildId == guild.Id && guild.TextChannels.Any(y => y.Id == x.ChannelId)).OrderBy(x => x.ChannelId).ToList();
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

        private static async Task PurgeChannelAsync(AutopurgeRow row)
        {
            try
            {
                row = await Database.Data.Autopurge.GetRowAsync(row.GuildId, row.ChannelId);
                if(row.Mode == 2) return;

                SocketGuild guild = _oldClient.GetGuild(row.GuildId);
                SocketTextChannel channel = guild.GetTextChannel(row.ChannelId);

                //if(!channel.BotHasPermissions(ChannelPermission.ViewChannel, ChannelPermission.ReadMessageHistory, ChannelPermission.ManageMessages)) return;

                List<AutopurgeMessageRow> messagesToDelete = await Database.Data.Autopurge.GetAndDeleteDueMessagesAsync(row);
                if(messagesToDelete.Count == 0) return;

                await channel.DeleteMessagesAsync(messagesToDelete.Select(x => x.MessageId), new RequestOptions { RetryMode = RetryMode.AlwaysRetry });
            }
            catch (Exception e)
            {
                _logger.ReportError("Autopurge", e);
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = PurgeChannelsAsync();
            _ = GetNewChannelsMessagesAsync();
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
            await Task.Delay(30000);
            List<AutopurgeRow> rows = await Database.Data.Autopurge.GetRowsAsync(enabledOnly: true);
            rows.RemoveAll(x => _oldClient.Guilds.All(y => y.Id != x.GuildId));
            rows.RemoveAll(x => _oldClient.GetGuild(x.GuildId).TextChannels.All(y => y.Id != x.ChannelId));

            _logger.Log("AutopurgeG", $"Started downloading messages for {rows.Count} channels");

            List<Task> tasks = new List<Task>();
            foreach (AutopurgeRow row in rows)
            {
                while (tasks.Count(x => !x.IsCompleted) >= 10) 
                    await Task.Delay(1000);

                tasks.Add(GetChannelMessagesAsync(row));
                await Task.Delay(250);
            }

            await Task.WhenAll(tasks);

            _logger.Log("AutopurgeG", "Finished downloading messages");
        }

        private static async Task GetNewChannelsMessagesAsync()
        {
            List<MiscRow> miscRows = await Misc.GetRowsAsync(type: "RequiresAutopurgeMessageDownload");
            miscRows.RemoveAll(x => _oldClient.Guilds.All(y => y.Id != x.GuildId));

            foreach (MiscRow miscRow in miscRows)
                _ = Misc.DeleteRowAsync(miscRow);

            foreach (MiscRow miscRow in miscRows)
            {
                AutopurgeRow row = await Database.Data.Autopurge.GetRowAsync(miscRow.GuildId, ulong.Parse(miscRow.Value));
                _ = GetChannelMessagesAsync(row);
            }
        }

        private static async Task GetChannelMessagesAsync(AutopurgeRow row)
        {
            await Task.Delay(1);
            
            lock (_downloadingFor)
            {
                if (_downloadingFor.Contains(row.ChannelId)) return;
                _downloadingFor.Add(row.ChannelId);
            }

            try
            {
                SocketGuild guild = _oldClient.GetGuild(row.GuildId);
                SocketTextChannel channel = guild.GetTextChannel(row.ChannelId);

                //if(!channel.BotHasPermissions(ChannelPermission.ViewChannel, ChannelPermission.ReadMessageHistory)) return;

                List<AutopurgeMessageRow> messageRows =
                    await Database.Data.Autopurge.GetMessagesAsync(guild.Id, channel.Id);

                List<IMessage> messages = new List<IMessage>();
                IMessage oldestMessage = null;

                while (true)
                {
                    List<IMessage> fetchedMessages;
                    if (oldestMessage is null)
                        fetchedMessages = (await channel.GetMessagesAsync().FlattenAsync()).ToList();
                    else
                        fetchedMessages =
                            (await channel.GetMessagesAsync(oldestMessage, Direction.Before).FlattenAsync()).ToList();

                    if (fetchedMessages.Count == 0) break;
                    oldestMessage = fetchedMessages.OrderBy(x => x.Timestamp.UtcDateTime).First();

                    messages.AddRange(fetchedMessages.Where(x =>
                        x.Timestamp.UtcDateTime > DateTime.UtcNow.AddDays(-13.9)));

                    if (messages.Count < 100 ||
                        oldestMessage.Timestamp.UtcDateTime < DateTime.UtcNow.AddDays(-13.9)) break;

                    await Task.Delay(1000);
                }

                foreach (IMessage message in messages)
                {
                    AutopurgeMessageRow messageRow = messageRows.FirstOrDefault(x => x.MessageId == message.Id);
                    if (messageRow is not null)
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
                        try { await Database.Data.Autopurge.SaveMessageAsync(messageRow); } catch { }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.ReportError("AutopurgeG", e);
            }
            finally
            {
                lock (_downloadingFor)
                {
                    _downloadingFor.Remove(row.ChannelId);
                }
            }
        }
    }
}
