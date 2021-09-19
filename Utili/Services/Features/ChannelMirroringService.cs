using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Http;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Database.Extensions;
using Utili.Extensions;

namespace Utili.Services
{
    public class ChannelMirroringService
    {
        private readonly ILogger<ChannelMirroringService> _logger;
        private readonly DiscordClientBase _client;

        private Dictionary<ulong, IWebhook> _webhookCache;

        public ChannelMirroringService(ILogger<ChannelMirroringService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;

            _webhookCache = new Dictionary<ulong, IWebhook>();
        }

        public async Task MessageReceived(IServiceScope scope, MessageReceivedEventArgs e)
        {
            try
            {
                if(!e.GuildId.HasValue || e.Message is not IUserMessage {WebhookId: null} userMessage) return;

                var db = scope.GetDbContext();
                var config = await db.ChannelMirroringConfigurations.GetForGuildChannelAsync(e.GuildId.Value, e.ChannelId);
                if (config is null) return;
                
                var guild = _client.GetGuild(e.GuildId.Value);
                var destinationChannel = guild.GetTextChannel(config.DestinationChannelId);
                if(destinationChannel is null) return;

                if(!destinationChannel.BotHasPermissions(Permission.ViewChannel | Permission.ManageWebhooks)) return;
                
                IWebhook webhook;
                try
                {
                    webhook = await GetWebhookAsync(config.WebhookId);
                    if (webhook?.ChannelId is null || webhook.ChannelId != destinationChannel.Id) webhook = null;
                }
                catch (RestApiException ex) when (ex.StatusCode == HttpResponseStatusCode.NotFound)
                {
                    webhook = null;
                }

                if (webhook is null)
                {
                    var avatar = File.OpenRead("Avatar.png");
                    webhook = await destinationChannel.CreateWebhookAsync("Utili Mirroring", x => x.Avatar = avatar);
                    avatar.Close();

                    config.WebhookId = webhook.Id;
                    db.ChannelMirroringConfigurations.Update(config);
                    await db.SaveChangesAsync();
                }

                var username = $"{e.Message.Author} in {e.Channel.Name}";
                var avatarUrl = e.Message.Author.GetAvatarUrl();
                
                if (!string.IsNullOrWhiteSpace(userMessage.Content) || userMessage.Embeds.Any(x => x.IsRich))
                {
                    var message = new LocalWebhookMessage()
                        .WithName(username)
                        .WithAvatarUrl(avatarUrl)
                        .WithOptionalContent(userMessage.Content)
                        .WithEmbeds(userMessage.Embeds.Where(x => x.IsRich).Select(LocalEmbed.FromEmbed))
                        .WithAllowedMentions(LocalAllowedMentions.None);

                    await _client.ExecuteWebhookAsync(webhook.Id, webhook.Token, message);
                }
                    
                foreach (var attachment in userMessage.Attachments)
                {
                    var attachmentMessage = new LocalWebhookMessage()
                        .WithName(username)
                        .WithAvatarUrl(avatarUrl)
                        .WithContent(attachment.Url);
                    await _client.ExecuteWebhookAsync(webhook.Id, webhook.Token, attachmentMessage);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received ({Guild}/{Channel}/{Message})", e.GuildId, e.ChannelId, e.MessageId);
            }
        }

        private async Task<IWebhook> GetWebhookAsync(ulong webhookId)
        {
            if (_webhookCache.TryGetValue(webhookId, out var cachedWebhook)) return cachedWebhook;
            try
            {
                var webhook = await _client.FetchWebhookAsync(webhookId);
                _webhookCache.TryAdd(webhookId, webhook);
                return webhook;
            }
            catch
            {
                return null;
            }
        }
    }
}
