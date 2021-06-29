using System;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Features
{
    public class VoteChannelsService
    {
        private readonly ILogger<VoteChannelsService> _logger;
        private readonly DiscordClientBase _client;

        public VoteChannelsService(ILogger<VoteChannelsService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }
        
        public async Task MessageReceived(MessageReceivedEventArgs e)
        {
            try
            {
                if (!e.GuildId.HasValue) return;
                    
                if (!e.Channel.BotHasPermissions(Permission.ViewChannel | Permission.ReadMessageHistory | Permission.AddReactions)) return;

                var row = (await VoteChannels.GetRowsAsync(e.GuildId.Value, e.ChannelId)).FirstOrDefault();
                if (row is null || !DoesMessageObeyRule(e.Message, row)) return;

                if (await Premium.IsGuildPremiumAsync(e.GuildId.Value)) row.Emotes = row.Emotes.Take(5).ToList();
                else row.Emotes = row.Emotes.Take(2).ToList();

                var guild = _client.GetGuild(e.GuildId.Value);
                foreach (var emojiString in row.Emotes)
                {
                    var emoji = LocalEmoji.FromEmoji(guild.GetEmoji(emojiString));
                    if (emoji is null) continue;
                    await e.Message.AddReactionAsync(emoji);
                }
                    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in message received");
            }
        }

        private static bool DoesMessageObeyRule(IMessage msg, VoteChannelsRow row)
        {
            return row.Mode switch
            {
                0 => // All
                    true,
                1 => // Images
                    msg is IUserMessage message && message.IsImage(),
                2 => // Videos
                    msg is IUserMessage message && message.IsVideo(),
                3 => // Media
                    msg is IUserMessage message && (message.IsImage() || message.IsVideo()),
                4 => // Music
                    msg is IUserMessage message && (message.IsMusic() || message.IsVideo()),
                5 => // Attachments
                    msg is IUserMessage message && message.IsAttachment(),
                6 => // URLs
                    msg is IUserMessage message && message.IsUrl(),
                7 => // URLs or Media
                    msg is IUserMessage message && (message.IsImage() || message.IsVideo() || message.IsUrl()),
                8 => // Embeds
                    msg is IUserMessage message && message.IsEmbed(),
                _ => false
            };
        }
    }
}
