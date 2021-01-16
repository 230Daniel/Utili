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
        private static Timer _premiumTimer;

        public static void Start()
        {
            _timer?.Dispose();
            _timer = new Timer(30000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();

            _premiumTimer?.Dispose();
            _premiumTimer = new Timer(10000);
            _premiumTimer.Elapsed += PremiumTimer_Elapsed;
            _premiumTimer.Start();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                PurgeChannelsAsync(false).GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {
                _logger.ReportError("Autopurge", ex);
            }
        }

        private static void PremiumTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                PurgeChannelsAsync(true).GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {
                _logger.ReportError("Autopurge", ex);
            }
        }

        private static async Task PurgeChannelsAsync(bool premium)
        {
            List<AutopurgeRow> rows;
            if (premium) rows = await GetPremiumRowsToPurgeAsync();
            else rows = await GetRowsToPurgeAsync();

            int messageCap;
            if (premium) messageCap = 500;
            else messageCap = 100;

            List<Task> tasks = new List<Task>();
            foreach (AutopurgeRow row in rows)
            {
                try
                {
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
                        await Task.Delay(250);
                    }
                }
                catch { }
            }

            await Task.WhenAll(tasks);
        }

        private static async Task PurgeChannelAsync(SocketGuildChannel guildChannel, TimeSpan timespan, int mode, int messageCap)
        {
            await Task.Delay(1);

            SocketTextChannel channel = guildChannel as SocketTextChannel;
            if(BotPermissions.IsMissingPermissions(channel, new [] { ChannelPermission.ManageMessages }, out _)) return;

            List<IMessage> messages = (await channel.GetMessagesAsync(messageCap).FlattenAsync()).ToList();
            bool exceedesCap = messages.Count == messageCap;
            if(messages.Count == 0) return;
            IMessage lastMessage = messages.Last();

            // These DateTimes represent the bounds for which messages should be deleted.
            // DateTimes are confusing as heck and I think these are named the wrong way around but the logic works so ¯\_(ツ)_/¯
            DateTime earliestTime = DateTime.UtcNow - timespan;
            DateTime latestTime = DateTime.UtcNow - TimeSpan.FromDays(13.5);

            messages.RemoveAll(x => x.CreatedAt.UtcDateTime > earliestTime);
            messages.RemoveAll(x => x.CreatedAt.UtcDateTime < latestTime);
            messages.RemoveAll(x => x.IsPinned);

            // Only delete bot messages
            if (mode == 1) messages.RemoveAll(x => !x.Author.IsBot);

            await channel.DeleteMessagesAsync(messages);

            if (exceedesCap && mode == 0 && lastMessage.CreatedAt < latestTime)
            {
                // We must delete excess messages
                // Only do this if we are to delete all messages, not just bot messages
                // Only do this if the earliest message in the channel is deletable

                List<IMessage> excessMessages = (await channel.GetMessagesAsync(lastMessage.Id, Direction.Before).FlattenAsync()).ToList();

                excessMessages.RemoveAll(x => x.CreatedAt.UtcDateTime < latestTime);
                excessMessages.RemoveAll(x => x.IsPinned);

                await channel.DeleteMessagesAsync(excessMessages);
            }
        }

        private static int _purgeNumber;
        private static async Task<List<AutopurgeRow>> GetRowsToPurgeAsync()
        {
            List<AutopurgeRow> rows = await Database.Data.Autopurge.GetRowsAsync(enabledOnly: true);
            List<PremiumRow> premiumRows = await Premium.GetRowsAsync();
            rows = rows.Where(x => premiumRows.All(y => y.GuildId != x.GuildId)).ToList();

            List<AutopurgeRow> rowsToPurge = new List<AutopurgeRow>();
            for (int i = 0; i < 1; i++)
            {
                foreach (SocketGuild guild in _client.Guilds)
                {
                    List<AutopurgeRow> guildRows = rows.Where(x => x.GuildId == guild.Id && guild.TextChannels.Any(y => y.Id == x.ChannelId)).OrderBy(x => x.ChannelId).ToList();
                    if (guildRows.Count > 0)
                    {
                        AutopurgeRow row = guildRows[_purgeNumber % guildRows.Count];
                        if(!rowsToPurge.Contains(row)) rowsToPurge.Add(row);
                    }
                }
            }

            _purgeNumber++;
            if (_purgeNumber == int.MaxValue) _purgeNumber = 0;

            return rowsToPurge;
        }

        private static int _premiumPurgeNumber;
        private static async Task<List<AutopurgeRow>> GetPremiumRowsToPurgeAsync()
        {
            List<AutopurgeRow> rows = await Database.Data.Autopurge.GetRowsAsync(enabledOnly: true);
            List<PremiumRow> premiumRows = await Premium.GetRowsAsync();
            rows = rows.Where(x => premiumRows.Any(y => y.GuildId == x.GuildId)).ToList();

            List<AutopurgeRow> rowsToPurge = new List<AutopurgeRow>();
            for (int i = 0; i < 5; i++)
            {
                foreach (SocketGuild guild in _client.Guilds)
                {
                    List<AutopurgeRow> guildRows = rows.Where(x => x.GuildId == guild.Id && guild.TextChannels.Any(y => y.Id == x.ChannelId)).OrderBy(x => x.ChannelId).ToList();
                    if (guildRows.Count > 0)
                    {
                        AutopurgeRow row = guildRows[_premiumPurgeNumber % guildRows.Count];
                        if(!rowsToPurge.Contains(row)) rowsToPurge.Add(row);
                    }
                }

                _premiumPurgeNumber++;
                if (_premiumPurgeNumber == int.MaxValue) _premiumPurgeNumber = 0;
            }

            return rowsToPurge;
        }
    }
}
