using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class ChannelMirroringService
    {
        ILogger<ChannelMirroringService> _logger;
        DiscordClientBase _client;

        Dictionary<ulong, IWebhook> _webhookCache;

        public ChannelMirroringService(ILogger<ChannelMirroringService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;

            _webhookCache = new Dictionary<ulong, IWebhook>();
        }

        public async Task MessageReceived(MessageReceivedEventArgs e)
        {
            try
            {
                if(!e.GuildId.HasValue || e.Message is not IUserMessage {WebhookId: null} userMessage) return;

                ChannelMirroringRow row = await ChannelMirroring.GetRowAsync(e.GuildId.Value, e.ChannelId);
                CachedGuild guild = _client.GetGuild(e.GuildId.Value);
                CachedTextChannel channel = guild.GetTextChannel(row.ToChannelId);
                if(channel is null) return;

                if(!channel.BotHasPermissions(Permission.ViewChannel | Permission.ManageWebhooks)) return;

                IWebhook webhook = await GetWebhookAsync(row.WebhookId);

                if (webhook is null)
                {
                    FileStream avatar = File.OpenRead("Avatar.png");
                    webhook = await channel.CreateWebhookAsync("Utili Mirroring", x => x.Avatar = avatar);
                    avatar.Close();

                    row.WebhookId = webhook.Id;
                    await row.SaveAsync();
                }

                string username = $"{e.Message.Author} in {e.Channel.Name}";
                string avatarUrl = e.Message.Author.GetAvatarUrl();

                if (!string.IsNullOrWhiteSpace(userMessage.Content) || userMessage.Embeds.Count > 0)
                {
                    LocalWebhookMessageBuilder message = new LocalWebhookMessageBuilder()
                        .WithName(username)
                        .WithAvatarUrl(avatarUrl)
                        .WithOptionalContent(userMessage.Content)
                        .WithEmbeds(userMessage.Embeds.Select(LocalEmbedBuilder.FromEmbed))
                        .WithMentions(LocalMentionsBuilder.None);

                    await _client.ExecuteWebhookAsync(webhook.Id, webhook.Token, message.Build());
                }
                    
                foreach (Attachment attachment in userMessage.Attachments)
                {
                    LocalWebhookMessageBuilder attachmentMessage = new LocalWebhookMessageBuilder()
                        .WithName(username)
                        .WithAvatarUrl(avatarUrl)
                        .WithContent(attachment.Url);
                    await _client.ExecuteWebhookAsync(webhook.Id, webhook.Token, attachmentMessage.Build());
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received");
            }
        }

        async Task<IWebhook> GetWebhookAsync(ulong webhookId)
        {
            if (_webhookCache.TryGetValue(webhookId, out IWebhook cachedWebhook)) return cachedWebhook;
            try
            {
                IWebhook webhook = await _client.FetchWebhookAsync(webhookId);
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
