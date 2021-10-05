using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Rest;
using Database;
using Database.Entities;
using Database.Extensions;
using Qmmands;
using Utili.Commands;
using Utili.Extensions;
using Utili.Implementations;

namespace Utili.Features
{
    public class MessagePinningCommands : MyDiscordGuildModuleBase
    {
        private readonly DatabaseContext _dbContext;
        
        public MessagePinningCommands(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }
        
        [Command("pin")]
        [DefaultCooldown(2, 5)]
        [RequireAuthorChannelPermissions(Permission.ManageMessages)]
        public Task<DiscordCommandResult> PinAsync(
            ulong messageId,
            [RequireBotParameterChannelPermissions(Permission.ViewChannels | Permission.ManageWebhooks)]
            ITextChannel pinChannel = null)
            => PinAsync(messageId, pinChannel, Context.Channel);
        
        [Command("pin")]
        [DefaultCooldown(2, 5)]
        public Task<DiscordCommandResult> PinAsync(
            [RequireAuthorParameterChannelPermissions(Permission.ViewChannels | Permission.ManageMessages)]
            IMessageGuildChannel channel,
            ulong messageId,
            [RequireBotParameterChannelPermissions(Permission.ViewChannels | Permission.ManageWebhooks)]
            ITextChannel pinChannel = null)
            => PinAsync(messageId, pinChannel, channel);

        private async Task<DiscordCommandResult> PinAsync(ulong messageId, ITextChannel pinChannel, IMessageGuildChannel channel)
        {
            var message = await channel.FetchMessageAsync(messageId) as IUserMessage;

            if (message is null)
            {
                return Failure("Error",
                    $"No message was found in {channel.Mention} with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
            }

            var config = await _dbContext.MessagePinningConfigurations.GetForGuildAsync(Context.GuildId);
            if (config is not null && config.PinMessages) await message.PinAsync(new DefaultRestRequestOptions {Reason = $"Message Pinning (manual by {Context.Message.Author} {Context.Message.Author.Id})"});

            pinChannel ??= config is null ? null : Context.Guild.GetTextChannel(config.PinChannelId);
            
            if (pinChannel is null && (config is not null && config.PinMessages))
            {
                return Success("Message pinned",
                    "Set a pin channel on the dashboard or specify one in the command if you want the message to be copied to another channel as well.");
            }
            if (pinChannel is null && (config is null || !config.PinMessages))
            {
                return Failure("Error",
                    "Message pinning is not enabled on this server.");
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

            return Success("Message pinned",
                $"The message was sent to {pinChannel.Mention}");
        }
    }
}
