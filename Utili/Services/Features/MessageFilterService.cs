using System;
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
    public class MessageFilterService
    {
        ILogger<MessageFilterService> _logger;
        DiscordClientBase _client;

        public MessageFilterService(ILogger<MessageFilterService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }

        public Task MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if(!e.GuildId.HasValue) return;

                    if(!e.Channel.BotHasPermissions(_client, Permission.ViewChannel | Permission.ManageMessages)) return;
                    if (e.Message is IUserMessage userMessage && 
                        e.Member.Id == _client.CurrentUser.Id &&
                        userMessage.Embeds.Count > 0 && 
                        userMessage.Embeds.First().Author?.Name == "Message deleted")
                        return;

                    MessageFilterRow row = await MessageFilter.GetRowAsync(e.GuildId.Value, e.ChannelId);
                    if(row.New) return;

                    if (e.Message is not IUserMessage)
                    {
                        await e.Message.DeleteAsync();
                        return;
                    }

                    if (!DoesMessageObeyRule(e.Message as IUserMessage, row, out string allowedTypes))
                    {
                        await e.Message.DeleteAsync();
                        if(e.Member.IsBot) return;

                        IUserMessage sent = await e.Channel.SendFailureAsync("Message deleted",
                            $"Only messages {allowedTypes} are allowed in {e.Channel.Mention}");
                        await Task.Delay(8000);
                        await sent.DeleteAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception thrown on message received");
                }
            });
            return Task.CompletedTask;
        }

        static bool DoesMessageObeyRule(IUserMessage message, MessageFilterRow row, out string allowedTypes)
        {
            switch (row.Mode)
            {
                case 0: // All
                    allowedTypes = "with anything";
                    return true;

                case 1: // Images
                    allowedTypes = "with images";
                    return message.IsImage();

                case 2: // Videos
                    allowedTypes = "with videos";
                    return message.IsVideo();

                case 3: // Media
                    allowedTypes = "with images or videos";
                    return message.IsImage() || message.IsVideo();

                case 4: // Music
                    allowedTypes = "with music";
                    return message.IsMusic() || message.IsVideo();

                case 5: // Attachments
                    allowedTypes = "with attachments";
                    return message.IsAttachment();

                case 6: // URLs
                    allowedTypes = "with valid urls";
                    return message.IsUrl();

                case 7: // URLs or Media
                    allowedTypes = "with images, videos or valid urls";
                    return message.IsImage() || message.IsVideo() || message.IsUrl();

                case 8: // RegEx
                    allowedTypes = "which match a custom expression";
                    return message.IsRegex(row.Complex.Value);

                default:
                    allowedTypes = "";
                    return true;
            }
        }
    }
}
