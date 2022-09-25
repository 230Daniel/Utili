using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Rest;
using Utili.Database;
using Utili.Database.Extensions;
using Disqord.Http;
using Qmmands;
using Qmmands.Text;
using Utili.Bot.Commands;
using Utili.Bot.Implementations;
using Utili.Bot.Services;
using Utili.Bot.Extensions;

namespace Utili.Bot.Features;

public class MessagePinningCommands : MyDiscordTextGuildModuleBase
{
    private readonly DatabaseContext _dbContext;
    private readonly WebhookService _webhookService;

    public MessagePinningCommands(DatabaseContext dbContext, WebhookService webhookService)
    {
        _dbContext = dbContext;
        _webhookService = webhookService;
    }

    [TextCommand("pin")]
    [DefaultRateLimit(2, 5)]
    [RequireAuthorPermissions(Permissions.ManageMessages)]
    public Task<IResult> PinAsync(
        ulong messageId,
        [RequireBotParameterChannelPermissions(Permissions.ViewChannels | Permissions.ManageWebhooks)]
        IMessageGuildChannel pinChannel = null)
        => PinAsync(messageId, pinChannel, Context.GetChannel());

    [TextCommand("pin")]
    [DefaultRateLimit(2, 5)]
    public Task<IResult> PinAsync(
        [RequireAuthorParameterChannelPermissions(Permissions.ViewChannels | Permissions.ManageMessages)]
        IMessageGuildChannel channel,
        ulong messageId,
        [RequireAuthorParameterChannelPermissions(Permissions.ViewChannels)]
        [RequireBotParameterChannelPermissions(Permissions.ViewChannels | Permissions.ManageWebhooks)]
        IMessageGuildChannel pinChannel = null)
        => PinAsync(messageId, pinChannel, channel);

    private async Task<IResult> PinAsync(ulong messageId, IMessageGuildChannel pinChannel, IMessageGuildChannel channel)
    {
        if (await channel.FetchMessageAsync(messageId) is not IUserMessage message)
        {
            return Failure("Error",
                $"No message was found in {channel.Mention} with ID {messageId}\n[How do I get a message ID?](https://support.discord.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-)");
        }

        var config = await _dbContext.MessagePinningConfigurations.GetForGuildAsync(Context.GuildId);
        if (config is not null && config.PinMessages && channel is ITextChannel)
            await message.PinAsync(new DefaultRestRequestOptions { Reason = $"Message Pinning (manual by {Context.Message.Author} {Context.Message.Author.Id})" });

        pinChannel ??= config is null ? null : Context.GetGuild().GetMessageGuildChannel(config.PinChannelId);

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

        var channelPrefix = channel.Type is ChannelType.Voice or ChannelType.Stage
            ? "🔈"
            : "#";
        var username = $"{message.Author} in {channelPrefix}{channel.Name}";
        var avatarUrl = message.Author.GetAvatarUrl();

        for (var i = 0; i < 2; i++)
        {
            var webhook = await _webhookService.GetWebhookAsync(pinChannel.Id);

            try
            {
                if (!string.IsNullOrWhiteSpace(message.Content) || message.Embeds.Count > 0)
                {
                    var messageBuilder = new LocalWebhookMessage()
                        .WithAuthorName(username)
                        .WithAuthorAvatarUrl(avatarUrl)
                        .WithOptionalContent(message.Content)
                        .WithEmbeds(message.Embeds.Select(LocalEmbed.CreateFrom))
                        .WithAllowedMentions(LocalAllowedMentions.None);

                    await Context.Bot.ExecuteWebhookAsync(webhook.Id, webhook.Token, messageBuilder);
                }

                foreach (var attachment in message.Attachments)
                {
                    var attachmentMessage = new LocalWebhookMessage()
                        .WithAuthorName(username)
                        .WithAuthorAvatarUrl(avatarUrl)
                        .WithContent(attachment.Url);
                    await Context.Bot.ExecuteWebhookAsync(webhook.Id, webhook.Token, attachmentMessage);
                }

                break;
            }
            catch (RestApiException ex) when (ex.StatusCode == HttpResponseStatusCode.NotFound)
            {
                await _webhookService.ReportInvalidWebhookAsync(pinChannel.Id, webhook.Id);
                if (i == 1) throw;
            }
        }

        return Success("Message pinned",
            $"The message was sent to {pinChannel.Mention}");
    }
}
