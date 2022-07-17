using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Utili.Database.Entities;
using Utili.Database.Extensions;
using Utili.Bot.Extensions;
using RepeatingTimer = System.Timers.Timer;
using Timer = System.Threading.Timer;

namespace Utili.Bot.Services;

public class NoticesService
{
    private readonly ILogger<NoticesService> _logger;
    private readonly DiscordClientBase _client;
    private readonly IServiceScopeFactory _scopeFactory;

    private Dictionary<Snowflake, Timer> _channelUpdateTimers = new();
    private RepeatingTimer _dashboardNoticeUpdateTimer;

    public NoticesService(ILogger<NoticesService> logger, DiscordClientBase client, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _client = client;
        _scopeFactory = scopeFactory;

        _dashboardNoticeUpdateTimer = new RepeatingTimer(3000);
        _dashboardNoticeUpdateTimer.Elapsed += DashboardNoticeUpdateTimer_Elapsed;
    }

    public void Start()
    {
        _dashboardNoticeUpdateTimer.Start();
    }

    public async Task MessageReceived(IServiceScope scope, MessageReceivedEventArgs e)
    {
        try
        {
            var db = scope.GetDbContext();
            var config = await db.NoticeConfigurations.GetForGuildChannelAsync(e.GuildId.Value, e.ChannelId);
            if (config is null) return;

            if (config.Enabled && e.Message is ISystemMessage && e.Message.Author.Id == _client.CurrentUser.Id)
            {
                await e.Message.DeleteAsync();
                return;
            }
            if (!config.Enabled || e.Message.Author.Id == _client.CurrentUser.Id) return;

            var delay = config.Delay;
            var minimumDelay = e.Member is null || e.Member.IsBot
                ? TimeSpan.FromSeconds(10)
                : TimeSpan.FromSeconds(5);
            if (delay < minimumDelay) delay = minimumDelay;

            ScheduleNoticeUpdate(e.GuildId.Value, e.ChannelId, delay);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown on message received ({Guild}/{Channel}/{Message})", e.GuildId, e.ChannelId, e.MessageId);
        }
    }

    private void DashboardNoticeUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        _ = UpdateNoticesFromDashboardAsync();
    }

    private async Task UpdateNoticesFromDashboardAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.GetDbContext();

            var updatedConfigs = await db.NoticeConfigurations.Where(x => x.UpdatedFromDashboard).ToListAsync();
            updatedConfigs.RemoveAll(x => _client.GetGuild(x.GuildId) is null);

            foreach (var updatedConfig in updatedConfigs)
            {
                updatedConfig.UpdatedFromDashboard = false;
                await db.SaveChangesAsync();
                ScheduleNoticeUpdate(updatedConfig.GuildId, updatedConfig.ChannelId, TimeSpan.FromSeconds(1));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown updating notices from dashboard");
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
            if (_channelUpdateTimers.TryGetValue(channelId, out var timer))
            {
                timer.Dispose();
                _channelUpdateTimers.Remove(channelId);
            }

            _channelUpdateTimers.Add(channelId, newTimer);
        }
    }

    private async Task UpdateNoticeAsync(Snowflake guildId, Snowflake channelId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.GetDbContext();
            var config = await db.NoticeConfigurations.GetForGuildChannelAsync(guildId, channelId);

            if (config is null || !config.Enabled) return;

            var guild = _client.GetGuild(guildId);
            var channel = guild.GetTextChannel(channelId);

            if (channel is null ||
                !channel.BotHasPermissions(
                    Permission.ViewChannels |
                    Permission.ReadMessageHistory |
                    Permission.ManageMessages |
                    Permission.SendMessages |
                    Permission.SendEmbeds |
                    Permission.SendAttachments)) return;

            var previousMessage = await channel.FetchMessageAsync(config.MessageId);
            if (previousMessage is not null) await previousMessage.DeleteAsync();

            var message = await channel.SendMessageAsync(GetNotice(config));

            config.MessageId = message.Id;
            db.NoticeConfigurations.Update(config);
            await db.SaveChangesAsync();

            if (config.Pin)
                await message.PinAsync(new DefaultRestRequestOptions { Reason = "Sticky Notices" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown while updating notice for {GuildId}/{ChannelId}", guildId, channelId);
        }
    }

    public static LocalMessage GetNotice(NoticeConfiguration config)
    {
        var text = config.Text.Replace(@"\n", "\n");
        var title = config.Title;
        var content = config.Content.Replace(@"\n", "\n");
        var footer = config.Footer.Replace(@"\n", "\n");

        var iconUrl = config.Icon;
        var thumbnailUrl = config.Thumbnail;
        var imageUrl = config.Image;

        if (!Uri.TryCreate(iconUrl, UriKind.Absolute, out var uriResult1) || uriResult1.Scheme is not ("http" or "https")) iconUrl = null;
        if (!Uri.TryCreate(thumbnailUrl, UriKind.Absolute, out var uriResult2) || uriResult2.Scheme is not ("http" or "https")) thumbnailUrl = null;
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uriResult3) || uriResult3.Scheme is not ("http" or "https")) imageUrl = null;

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
                .WithColor(new Color((int)config.Colour)));
    }
}