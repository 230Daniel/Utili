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

        public async Task<bool> MessageReceived(MessageReceivedEventArgs e)
        // Returns true if the message was deleted by the filter
        {
            try
            {
                if(!e.GuildId.HasValue) return false;

                if(!e.Channel.BotHasPermissions(Permission.ViewChannel | Permission.ManageMessages)) return false;
                if (e.Message is IUserMessage userMessage && 
                    e.Member is not null &&
                    e.Member.Id == _client.CurrentUser.Id &&
                    userMessage.Embeds.Count > 0 && 
                    userMessage.Embeds[0].Author?.Name == "Message deleted")
                    return false;

                MessageFilterRow row = await MessageFilter.GetRowAsync(e.GuildId.Value, e.ChannelId);
                if(row.New) return false;

                if (e.Message is not IUserMessage message)
                {
                    await e.Message.DeleteAsync();
                    return false;
                }

                if (!DoesMessageObeyRule(message, row, out string allowedTypes))
                {
                    await e.Message.DeleteAsync();
                    if(e.Member.IsBot) return true;

                    IUserMessage sent = await e.Channel.SendFailureAsync("Message deleted",
                        $"Only messages {allowedTypes} are allowed in {e.Channel.Mention}");
                    await Task.Delay(8000);
                    await sent.DeleteAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received");
            }
            return false;
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
