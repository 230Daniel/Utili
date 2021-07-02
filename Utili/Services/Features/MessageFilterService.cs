﻿using System;
using System.Collections.Concurrent;
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
        public async Task<bool> MessageReceived(MessageReceivedEventArgs e)
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

                var row = await MessageFilter.GetRowAsync(e.GuildId.Value, e.ChannelId);
                if(row.New) return false;

                if (e.Message is not IUserMessage message)
                {
                    await e.Message.DeleteAsync(new DefaultRestRequestOptions {Reason = "Message Filter"});
                    return false;
                }

                if (!DoesMessageObeyRule(message, row, out var allowedTypes))
                {
                    await e.Message.DeleteAsync(new DefaultRestRequestOptions {Reason = "Message Filter"});
                    if(e.Member is null || e.Member.IsBot) return true;

                    if (_offenceDictionary.TryGetValue(e.ChannelId, out var recentOffence) && recentOffence > DateTime.UtcNow.AddSeconds(-4))
                        return true;
                    
                    _offenceDictionary.AddOrUpdate(e.ChannelId, DateTime.UtcNow, (_, _) => DateTime.UtcNow);

                    var sent = await e.Channel.SendFailureAsync("Message deleted",
                        $"Only messages {allowedTypes} are allowed in {e.Channel.Mention}");
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

        private static bool DoesMessageObeyRule(IUserMessage message, MessageFilterRow row, out string allowedTypes)
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
