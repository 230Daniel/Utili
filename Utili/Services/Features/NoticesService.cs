using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;
using RepeatingTimer = System.Timers.Timer;
using Timer = System.Threading.Timer;

namespace Utili.Services
{
    public class NoticesService
    {
        private readonly ILogger<NoticesService> _logger;
        private readonly DiscordClientBase _client;

        private List<(Snowflake, Timer)> _channelUpdateTimers = new();
        private RepeatingTimer _dashboardNoticeUpdateTimer;

        public NoticesService(ILogger<NoticesService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }

        public void Start()
        {
            _dashboardNoticeUpdateTimer = new RepeatingTimer(3000);
            _dashboardNoticeUpdateTimer.Elapsed += DashboardNoticeUpdateTimer_Elapsed;
        }

        public async Task MessageReceived(MessageReceivedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue) return;

                var row = await Notices.GetRowAsync(e.GuildId.Value, e.ChannelId);
                if (row.Enabled && e.Message is ISystemMessage && e.Message.Author.Id == _client.CurrentUser.Id)
                {
                    await e.Message.DeleteAsync();
                    return;
                }
                if (!row.Enabled || e.Message.Author.Id == _client.CurrentUser.Id) return;
                
                var delay = row.Delay;
                var minimumDelay = e.Member is null || e.Member.IsBot
                    ? TimeSpan.FromSeconds(10)
                    : TimeSpan.FromSeconds(5);
                if (delay < minimumDelay) delay = minimumDelay;

                ScheduleNoticeUpdate(e.GuildId.Value, e.ChannelId, delay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received");
            }
        }

        private void DashboardNoticeUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = UpdateNoticesFromDashboardAsync();
        }

        private async Task UpdateNoticesFromDashboardAsync()
        {
            var fromDashboard = await Misc.GetRowsAsync(type: "RequiresNoticeUpdate");
            fromDashboard.RemoveAll(x => _client.GetGuilds().All(y => x.GuildId != y.Key));
            foreach (var miscRow in fromDashboard)
            {
                await Misc.DeleteRowAsync(miscRow);
                ScheduleNoticeUpdate(miscRow.GuildId, ulong.Parse(miscRow.Value), TimeSpan.FromSeconds(1));
            }
        }

        private void ScheduleNoticeUpdate(Snowflake guildId, Snowflake channelId, TimeSpan delay)
        {
            Timer newTimer = new(x =>
            {
                _ = UpdateNoticeAsync(guildId, channelId);
            }, this, delay, Timeout.InfiniteTimeSpan);

            lock (_channelUpdateTimers)
            {
                var channelUpdate = _channelUpdateTimers.FirstOrDefault(x => x.Item1 == channelId);
                if (channelUpdate.Item2 is not null)
                {
                    channelUpdate.Item2.Dispose();
                    _channelUpdateTimers.Remove(channelUpdate);
                }

                _channelUpdateTimers.Add((channelId, newTimer));
            }
        }

        private async Task UpdateNoticeAsync(Snowflake guildId, Snowflake channelId)
        {
            try
            {
                var row = await Notices.GetRowAsync(guildId, channelId);
                if (row is null || !row.Enabled) return;

                IGuild guild = _client.GetGuild(guildId);
                ITextChannel channel = guild.GetTextChannel(channelId);

                if (!channel.BotHasPermissions(
                    Permission.ViewChannel |
                    Permission.ReadMessageHistory |
                    Permission.ManageMessages |
                    Permission.SendMessages |
                    Permission.EmbedLinks |
                    Permission.AttachFiles)) return;

                var previousMessage = await channel.FetchMessageAsync(row.MessageId);
                if(previousMessage is not null) await previousMessage.DeleteAsync();

                var message = await channel.SendMessageAsync(GetNotice(row));
                row.MessageId = message.Id;
                await Notices.SaveMessageIdAsync(row);
                await message.PinAsync(new DefaultRestRequestOptions {Reason = "Sticky Notices"});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while updating notice for {GuildId}/{ChannelId}", guildId, channelId);
            }
        }

        public static LocalMessage GetNotice(NoticesRow row)
        {
            var text = row.Text.Value.Replace(@"\n", "\n");
            var title = row.Title.Value;
            var content = row.Content.Value.Replace(@"\n", "\n");
            var footer = row.Footer.Value.Replace(@"\n", "\n");

            var iconUrl = row.Icon.Value;
            var thumbnailUrl = row.Thumbnail.Value;
            var imageUrl = row.Image.Value;

            if (string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(iconUrl))
                title = "Title";

            if (string.IsNullOrWhiteSpace(title) &&
                string.IsNullOrWhiteSpace(content) &&
                string.IsNullOrWhiteSpace(footer) &&
                string.IsNullOrWhiteSpace(iconUrl) &&
                string.IsNullOrWhiteSpace(thumbnailUrl) &&
                string.IsNullOrWhiteSpace(imageUrl))
            {
                return new LocalMessage()
                    .WithRequiredContent(text);
            }
            
            return new LocalMessage()
                .WithOptionalContent(text)
                .AddEmbed(new LocalEmbed()
                    .WithOptionalAuthor(title, iconUrl)
                    .WithDescription(content)
                    .WithOptionalFooter(footer)
                    .WithThumbnailUrl(thumbnailUrl)
                    .WithImageUrl(imageUrl)
                    .WithColor(new Color((int) row.Colour)));
        }
    }
}
