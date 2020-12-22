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
        private static Timer _timer;
        private static int _freeCounter;

        public static void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(10000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool premiumOnly;

            _freeCounter++;

            if (_freeCounter == 3)
            {
                premiumOnly = false;
                _freeCounter = 0;
            }
            else
            {
                premiumOnly = true;
            }

            _ = PurgeChannelsAsync(premiumOnly);
        }

        private static async Task PurgeChannelsAsync(bool premiumOnly)
        {
            // TODO: Make premium be 5 channels per 10 seconds or something, because someone is purging a rediculous 70 channels!
            List<AutopurgeRow> rows = await Database.Data.Autopurge.GetRowsAsync();
            List<ulong> allGuildIds = _client.Guilds.Select(x => x.Id).ToList();
            List<ulong> premiumGuildIds = (await Premium.GetRowsAsync()).Select(x => x.GuildId).Distinct().ToList();

            // Get only rows handled by this cluster
            rows.RemoveAll(x => !allGuildIds.Contains(x.GuildId));

            if (premiumOnly)
            {
                // Get only premium rows
                rows.RemoveAll(x => !premiumGuildIds.Contains(x.GuildId));
            }
            else
            {
                // If it's a free run, get a selection of non-premium channels
                List<AutopurgeRow> selectedNonPremiumRows = GetChannelsForThisPurge(rows.Where(x => !premiumGuildIds.Contains(x.GuildId)).ToList());

                // Get only premium rows
                rows.RemoveAll(x => !premiumGuildIds.Contains(x.GuildId));

                // Add the selection of non-premium channels
                rows.AddRange(selectedNonPremiumRows);
            }

            // Remove rows in mode 2 (disabled)
            rows.RemoveAll(x => x.Mode == 2);

            List<Task> tasks = new List<Task>();

            foreach (AutopurgeRow row in rows)
            {
                try
                {
                    int messageCap = 100;
                    if (premiumGuildIds.Contains(row.GuildId))
                    {
                        messageCap = 500;
                    }

                    SocketGuild guild = _client.GetGuild(row.GuildId);
                    SocketTextChannel channel = guild.GetTextChannel(row.ChannelId);

                    if (!BotPermissions.IsMissingPermissions(channel,
                        new[]
                        {
                            ChannelPermission.ViewChannel,
                            ChannelPermission.ReadMessageHistory,
                            ChannelPermission.ManageMessages
                        }, out _))
                    {
                        tasks.Add(PurgeChannelAsync(channel, row.Timespan, row.Mode, messageCap));
                    }
                }
                catch { }
            }

            await Task.WhenAll(tasks);
        }

        private static async Task PurgeChannelAsync(SocketGuildChannel guildChannel, TimeSpan timespan, int mode, int messageCap)
        {
            await Task.Delay(1);
            if(mode == 2) return; // jic above method is wrong

            SocketTextChannel channel = guildChannel as SocketTextChannel;

            if(BotPermissions.IsMissingPermissions(channel, new [] { ChannelPermission.ManageMessages }, out _)) return;

            List<IMessage> messages = (await channel.GetMessagesAsync(messageCap).FlattenAsync()).ToList();
            bool exceedesCap = messages.Count == messageCap;
            IMessage lastMessage = messages.Last();

            // These DateTimes represent the bounds for which messages should be deleted.
            // DateTimes are confusing as heck and I think these are named the wrong way around but the logic works so ¯\_(ツ)_/¯
            DateTime earliestTime = DateTime.UtcNow - timespan;
            DateTime latestTime = DateTime.UtcNow - TimeSpan.FromDays(13.9);

            messages.RemoveAll(x => x.CreatedAt.UtcDateTime > earliestTime);
            messages.RemoveAll(x => x.CreatedAt.UtcDateTime < latestTime);
            messages.RemoveAll(x => x.IsPinned);

            // Only delete bot messages
            if (mode == 1) messages.RemoveAll(x => !x.Author.IsBot);

            await channel.DeleteMessagesAsync(messages);

            if (exceedesCap && mode == 0 && lastMessage.CreatedAt > latestTime)
            {
                // We must delete excess messages
                // Only do this if we are to delete all messages, not just bot messages
                // Only do this if the earliest message in the channel is deletable

                List<IMessage> excessMessages = (await channel.GetMessagesAsync(lastMessage.Id, Direction.Before).FlattenAsync()).ToList();

                messages.RemoveAll(x => x.CreatedAt.UtcDateTime > latestTime);

                await channel.DeleteMessagesAsync(excessMessages);
            }
        }

        private static int _purgeNumber;
        private static List<AutopurgeRow> GetChannelsForThisPurge(List<AutopurgeRow> rows)
        {
            List<AutopurgeRow> channelsForThisPurge = new List<AutopurgeRow>();

            List<AutopurgeRow> clonedRows = new List<AutopurgeRow>();
            clonedRows.AddRange(rows);

            foreach (AutopurgeRow row in clonedRows)
            {
                if (channelsForThisPurge.All(x => x.GuildId != row.GuildId))
                {
                    List<AutopurgeRow> guildRows = rows.Where(x => x.GuildId == row.GuildId).OrderBy(x => x.ChannelId).ToList();

                    if (_purgeNumber == 0)
                    {
                        channelsForThisPurge.Add(guildRows[0]);
                    }
                    else
                    {
                        channelsForThisPurge.Add(guildRows[_purgeNumber % guildRows.Count]);
                    }
                }
            }

            _purgeNumber++;
            if (_purgeNumber == int.MaxValue)
            {
                _purgeNumber = 0;
            }

            return channelsForThisPurge;
        }
    }
}
