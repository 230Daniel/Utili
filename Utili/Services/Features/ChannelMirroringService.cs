using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;
using Discord.WebSocket;
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

        public async Task MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                if(!e.GuildId.HasValue || e.Message is not IUserMessage userMessage || userMessage.WebhookId.HasValue) return;

                ChannelMirroringRow row = await ChannelMirroring.GetRowAsync(e.GuildId.Value, e.ChannelId);
                CachedGuild guild = _client.GetGuild(e.GuildId.Value);
                CachedTextChannel channel = guild.GetTextChannel(row.ToChannelId);
                if(channel is null) return;

                if(!channel.BotHasPermissions(_client, Permission.ViewChannel, Permission.ManageWebhooks)) return;

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

                LocalWebhookMessageBuilder message = new LocalWebhookMessageBuilder()
                    .WithName(username)
                    .WithAvatarUrl(avatarUrl)
                    .WithEmbeds((e.Message as IUserMessage).Embeds.Select(x => x.ToLocalEmbedBuilder()))
                    .WithMentions(LocalMentionsBuilder.None);

                if (!string.IsNullOrWhiteSpace(e.Message.Content)) message.Content = e.Message.Content;

                foreach (Attachment attachment in (e.Message as IUserMessage).Attachments)
                {
                    WebRequest request = WebRequest.Create(attachment.Url);
                    Stream stream = (await request.GetResponseAsync()).GetResponseStream();

                    LocalWebhookMessageBuilder attachmentMessage = new LocalWebhookMessageBuilder()
                        .WithAttachment(new LocalAttachment(stream, attachment.Filename));
                    // TODO: Send attachment message

                    stream?.Close();
                }
            });
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
