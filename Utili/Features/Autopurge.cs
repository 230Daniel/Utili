using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Database;
using Database.Data;
using Discord;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Features
{
    internal class Autopurge
    {
        private Timer _timer;
        private int _freeCounter;

        public void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(10000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
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

            _ = PurgeChannels(premiumOnly);
        }

        private async Task PurgeChannels(bool premiumOnly)
        {
            List<AutopurgeRow> rows = Database.Data.Autopurge.GetRows();
            List<ulong> allGuildIds = _client.Guilds.Select(x => x.Id).ToList();
            List<ulong> premiumGuildIds = Premium.GetPremiumGuilds();

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

            // Remove rows in mode 3 (disabled)
            rows.RemoveAll(x => x.Mode == 3);

            foreach (AutopurgeRow row in rows)
            {
                try
                {
                    int messageCap = 100;
                    if (Premium.IsPremium(row.GuildId))
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
                        await PurgeChannel(channel, row.Timespan, row.Mode, messageCap);
                    }
                }
                catch { }
            }
        }

        private async Task PurgeChannel(SocketGuildChannel guildChannel, TimeSpan timespan, int mode, int messageCap)
        {
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

            if (mode == 1)
            {
                // Mode = 1 so only delete bot messages
                messages.RemoveAll(x => !x.Author.IsBot);
            }

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

        private int _purgeNumber;
        private List<AutopurgeRow> GetChannelsForThisPurge(List<AutopurgeRow> rows)
        {
            List<AutopurgeRow> channelsForThisPurge = new List<AutopurgeRow>();

            List<AutopurgeRow> clonedRows = new List<AutopurgeRow>();
            clonedRows.AddRange(rows);

            foreach (AutopurgeRow row in clonedRows)
            {
                if (!channelsForThisPurge.Select(x => x.GuildId).Contains(row.GuildId))
                {
                    List<AutopurgeRow> guildRows = rows.Where(x => x.GuildId == row.GuildId).OrderBy(x => x.Id).ToList();

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
