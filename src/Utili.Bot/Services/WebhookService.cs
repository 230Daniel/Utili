using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Rest;
using Microsoft.Extensions.Logging;

namespace Utili.Bot.Services;

public class WebhookService
{
    private readonly ILogger<WebhookService> _logger;
    private readonly DiscordClientBase _client;
    private readonly Dictionary<Snowflake, IWebhook> _webhooks;
    private readonly SemaphoreSlim _semaphore;

    public WebhookService(ILogger<WebhookService> logger, DiscordClientBase client)
    {
        _logger = logger;
        _client = client;
        _webhooks = new Dictionary<Snowflake, IWebhook>();
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public async Task<IWebhook> GetWebhookAsync(Snowflake channelId)
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_webhooks.TryGetValue(channelId, out var cachedWebhook))
                return cachedWebhook;

            var webhook = await FetchOrCreateWebhookAsync(channelId);
            _webhooks[channelId] = webhook;
            return webhook;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ReportInvalidWebhookAsync(Snowflake channelId, Snowflake reportedWebhookId)
    {
        await _semaphore.WaitAsync();

        try
        {
            var cachedWebhook = _webhooks[channelId];
            if (cachedWebhook is null || cachedWebhook.Id != reportedWebhookId)
            {
                _logger.LogInformation("Invalid webhook {ReportedWebhookId} reported for {ChannelId}, dismissed as webhook {CachedWebhookId} is cached",
                    reportedWebhookId, channelId, cachedWebhook.Id);
                return;
            }

            var webhook = await FetchOrCreateWebhookAsync(channelId);
            _logger.LogInformation("Invalid webhook {ReportedWebhookId} reported for {ChannelId}, replaced with webhook {WebhookId}",
                reportedWebhookId, channelId, webhook.Id);
            _webhooks[channelId] = webhook;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<IWebhook> FetchOrCreateWebhookAsync(Snowflake channelId)
    {
        var webhooks = await _client.FetchChannelWebhooksAsync(channelId);

        var botWebhook = webhooks.FirstOrDefault(x => x.Creator.Id == _client.CurrentUser.Id);
        if (botWebhook is not null)
        {
            return botWebhook;
        }

        if (webhooks.Count == 10)
        {
            return webhooks[0];
        }

        var avatar = File.OpenRead("Avatar.png");
        var newWebhook = await _client.CreateWebhookAsync(channelId, "Utili", x => x.Avatar = avatar);
        avatar.Close();

        return newWebhook;
    }
}
