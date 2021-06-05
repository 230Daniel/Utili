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
        readonly ILogger<VoteChannelsService> _logger;
        readonly DiscordClientBase _client;

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

                VoteChannelsRow row = (await VoteChannels.GetRowsAsync(e.GuildId.Value, e.ChannelId)).FirstOrDefault();
                if (row is null || !DoesMessageObeyRule(e.Message as IUserMessage, row)) return;

                if (await Premium.IsGuildPremiumAsync(e.GuildId.Value)) row.Emotes = row.Emotes.Take(5).ToList();
                else row.Emotes = row.Emotes.Take(2).ToList();

                CachedGuild guild = _client.GetGuild(e.GuildId.Value);
                foreach (string emoji in row.Emotes)
                    await e.Message.AddReactionAsync(LocalEmoji.FromEmoji(guild.GetEmoji(emoji)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown in message received");
            }
        }

        static bool DoesMessageObeyRule(IUserMessage message, VoteChannelsRow row)
        {
            return row.Mode switch
            {
                0 => // All
                    true,
                1 => // Images
                    message.IsImage(),
                2 => // Videos
                    message.IsVideo(),
                3 => // Media
                    message.IsImage() || message.IsVideo(),
                4 => // Music
                    message.IsMusic() || message.IsVideo(),
                5 => // Attachments
                    message.IsAttachment(),
                6 => // URLs
                    message.IsUrl(),
                7 => // URLs or Media
                    message.IsImage() || message.IsVideo() || message.IsUrl(),
                8 => // Embeds
                    message.IsEmbed(),
                _ => false
            };
        }
    }
}
