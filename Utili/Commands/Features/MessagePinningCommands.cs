using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using NewDatabase;
using NewDatabase.Entities;
using NewDatabase.Extensions;
using Qmmands;
using Utili.Extensions;
using Utili.Implementations;

namespace Utili.Features
{
    public class MessagePinningCommands : DiscordGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;
        
        public MessagePinningCommands(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [Command("Pin")]
        [DefaultCooldown(2, 5)]
        [RequireAuthorChannelPermissions(Permission.ManageMessages)]
        public async Task Pin(
            ulong messageId,
            [RequireBotParameterChannelPermissions(Permission.ViewChannel | Permission.ManageWebhooks)]
            ITextChannel pinChannel = null)
            => await Pin(messageId, pinChannel, Context.Channel);
        
        [Command("Pin")]
        [DefaultCooldown(2, 5)]
        public async Task Pin(
            [RequireAuthorParameterChannelPermissions(Permission.ViewChannel | Permission.ManageMessages)]
            ITextChannel channel,
            ulong messageId,
            [RequireBotParameterChannelPermissions(Permission.ViewChannel | Permission.ManageWebhooks)]
            ITextChannel pinChannel = null)
            => await Pin(messageId, pinChannel, channel);

        private async Task Pin(ulong messageId, ITextChannel pinChannel, ITextChannel channel)
        {
            var message = await channel.FetchMessageAsync(messageId) as IUserMessage;

            if (message is null)
            {
                await Context.Channel.SendFailureAsync("Error",
                    $"No message was found in {channel.Mention} with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
                return;
            }

            var config = await _dbContext.MessagePinningConfigurations.GetForGuildAsync(Context.GuildId);
            if (config is not null && config.PinMessages) await message.PinAsync(new DefaultRestRequestOptions {Reason = $"Message Pinning (manual by {Context.Message.Author} {Context.Message.Author.Id})"});

            pinChannel ??= config is null ? null : Context.Guild.GetTextChannel(config.PinChannelId);
            
            if (pinChannel is null && (config is not null && config.PinMessages))
            {
                await Context.Channel.SendSuccessAsync("Message pinned",
                    "Set a pin channel on the dashboard or specify one in the command if you want the message to be copied to another channel as well.");
                return;
            }
            if (pinChannel is null && (config is null || !config.PinMessages))
            {
                await Context.Channel.SendFailureAsync("Error",
                    "Message pinning is not enabled on this server.");
                return;
            }

            IWebhook webhook = null;
            var webhookInfo = await _dbContext.MessagePinningWebhooks.GetForGuildChannelAsync(Context.Guild.Id, pinChannel.Id);
            if(webhookInfo is not null) webhook = await pinChannel.FetchWebhookAsync(webhookInfo.WebhookId);
            
            if (webhook is null || webhook.ChannelId != pinChannel.Id)
            {
                var avatar = File.OpenRead("Avatar.png");
                webhook = await pinChannel.CreateWebhookAsync("Utili Message Pinning", x =>
                {
                    x.Avatar = avatar;
                }, new DefaultRestRequestOptions {Reason = "Message Pinning"});
                avatar.Close();

                if (webhookInfo is null)
                {
                    webhookInfo = new MessagePinningWebhook(Context.GuildId, pinChannel.Id)
                    {
                        WebhookId = webhook.Id
                    };
                    _dbContext.MessagePinningWebhooks.Add(webhookInfo);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    webhookInfo.WebhookId = webhook.Id;
                    _dbContext.MessagePinningWebhooks.Update(webhookInfo);
                    await _dbContext.SaveChangesAsync();
                }
            }
            
            var username = $"{message.Author} in {channel.Name}";
            var avatarUrl = message.Author.GetAvatarUrl();

            if (!string.IsNullOrWhiteSpace(message.Content) || message.Embeds.Count > 0)
            {
                var messageBuilder = new LocalWebhookMessage()
                    .WithName(username)
                    .WithAvatarUrl(avatarUrl)
                    .WithOptionalContent(message.Content)
                    .WithEmbeds(message.Embeds.Select(LocalEmbed.FromEmbed))
                    .WithAllowedMentions(LocalAllowedMentions.None);

                await Context.Bot.ExecuteWebhookAsync(webhook.Id, webhook.Token, messageBuilder);
            }
            
            foreach (var attachment in message.Attachments)
            {
                var attachmentMessage = new LocalWebhookMessage()
                    .WithName(username)
                    .WithAvatarUrl(avatarUrl)
                    .WithContent(attachment.Url);
                await Context.Bot.ExecuteWebhookAsync(webhook.Id, webhook.Token, attachmentMessage);
            }

            await Context.Channel.SendSuccessAsync("Message pinned",
                $"The message was sent to {pinChannel.Mention}");
        }
    }
}
