using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Database.Entities;
using Database.Extensions;
using Utili.Extensions;

namespace Utili.Services
{
    public class MessageFilterService
    {
        private readonly ILogger<MessageFilterService> _logger;
        private readonly DiscordClientBase _client;
        
        private ConcurrentDictionary<Snowflake, DateTime> _offenceDictionary;
        
        public MessageFilterService(ILogger<MessageFilterService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
            _offenceDictionary = new();
        }
        
        /// <returns>True if the message was deleted by the filter</returns>
        public async Task<bool> MessageReceived(IServiceScope scope, MessageReceivedEventArgs e)
        {
            try
            {
                if((e.Message as IUserMessage)?.Type == UserMessageType.ThreadStarterMessage
                   || !e.Channel.BotHasPermissions(Permission.ViewChannels | Permission.ManageMessages)) 
                    return false;

                var userMessage = e.Message as IUserMessage;
                if (userMessage is not null && 
                    e.Member is not null &&
                    e.Member.Id == _client.CurrentUser.Id &&
                    userMessage.Embeds.Count > 0 && 
                    userMessage.Embeds[0].Author?.Name == "Message deleted")
                    return false;
                if (userMessage?.WebhookId is not null) return false;
                
                var db = scope.GetDbContext();
                var configChannelId = (e.Channel as IThreadChannel)?.ChannelId ?? e.ChannelId;
                var config = await db.MessageFilterConfigurations.GetForGuildChannelAsync(e.GuildId.Value, configChannelId);
                if (config is null || config.Mode == MessageFilterMode.All || e.Channel is IThreadChannel && !config.EnforceInThreads) return false;

                if (e.Message is not IUserMessage message)
                {
                    await e.Message.DeleteAsync(new DefaultRestRequestOptions {Reason = "Message Filter"});
                    return true;
                }

                if (!DoesMessageObeyRule(message, config.Mode, config.RegEx, out var allowedTypes))
                {
                    await e.Message.DeleteAsync(new DefaultRestRequestOptions {Reason = "Message Filter"});
                    if(e.Member is null || e.Member.IsBot) return true;

                    if (_offenceDictionary.TryGetValue(e.ChannelId, out var recentOffence) && recentOffence > DateTime.UtcNow.AddSeconds(-4))
                        return true;
                    
                    _offenceDictionary.AddOrUpdate(e.ChannelId, DateTime.UtcNow, (_, _) => DateTime.UtcNow);
                    
                    var deletionMessage = string.IsNullOrWhiteSpace(config.DeletionMessage)
                        ? allowedTypes.Contains(",")
                            ? $"Your message must contain one of `{allowedTypes}` to be allowed in {e.Channel.Mention}"
                            : $"Your message must contain `{allowedTypes}` to be allowed in {e.Channel.Mention}"
                        : config.DeletionMessage;
                    
                    var sent = await e.Channel.SendFailureAsync("Message deleted", deletionMessage);
                    await Task.Delay(8000);
                    await sent.DeleteAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on message received ({Guild}/{Channel}/{Message})", e.GuildId, e.ChannelId, e.MessageId);
            }
            return false;
        }

        private static bool DoesMessageObeyRule(IUserMessage message, MessageFilterMode mode, string regEx, out string allowedTypes)
        {
            allowedTypes = mode.ToString();
            
            if ((mode & MessageFilterMode.All) != 0) return true;
            if ((mode & MessageFilterMode.Images) != 0 && message.IsImage()) return true;
            if ((mode & MessageFilterMode.Videos) != 0 && message.IsVideo()) return true;
            if ((mode & MessageFilterMode.Music) != 0 && message.IsMusic()) return true;
            if ((mode & MessageFilterMode.Attachments) != 0 && message.IsAttachment()) return true;
            if ((mode & MessageFilterMode.Links) != 0 && message.IsLink()) return true;
            if ((mode & MessageFilterMode.RegEx) != 0 && message.IsRegex(regEx)) return true;

            return false;
        }
    }
}
