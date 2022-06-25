using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Utili.Database.Entities;
using Utili.Database.Extensions;
using Utili.Bot.Extensions;
using Utili.Bot.Services;

namespace Utili.Bot.Features
{
    public class VoteChannelsService
    {
        private readonly ILogger<VoteChannelsService> _logger;
        private readonly DiscordClientBase _client;
        private readonly IsPremiumService _isPremiumService;

        public VoteChannelsService(ILogger<VoteChannelsService> logger, DiscordClientBase client, IsPremiumService isPremiumService)
        {
            _logger = logger;
            _client = client;
            _isPremiumService = isPremiumService;
        }

        public async Task MessageReceived(IServiceScope scope, MessageReceivedEventArgs e)
        {
            try
            {
                if (!e.Channel.BotHasPermissions(Permission.ViewChannels | Permission.ReadMessageHistory | Permission.AddReactions) || e.Message is ISystemMessage && e.Message.Author.Id == _client.CurrentUser.Id) return;

                var db = scope.GetDbContext();
                var config = await db.VoteChannelConfigurations.GetForGuildChannelAsync(e.GuildId.Value, e.ChannelId);
                if (config is null || !DoesMessageObeyRule(e.Message as IUserMessage, config.Mode)) return;

                if (config.Emojis.Count > 2)
                {
                    var premium = await _isPremiumService.GetIsGuildPremiumAsync(e.GuildId.Value);
                    config.Emojis = config.Emojis.Take(premium ? 5 : 2).ToList();
                }

                var guild = _client.GetGuild(e.GuildId.Value);
                foreach (var emojiString in config.Emojis)
                {
                    var emoji = guild.GetEmoji(emojiString);
                    if (emoji is null) continue;
                    await e.Message.AddReactionAsync(LocalEmoji.FromEmoji(emoji));
                }
            }
            catch (Exception ex) when (ex is not RestApiException { Message: "Unknown Message" })
            {
                _logger.LogError(ex, "Exception thrown in message received ({Guild}/{Channel}/{Message})", e.GuildId, e.ChannelId, e.MessageId);
            }
        }

        private static bool DoesMessageObeyRule(IUserMessage message, VoteChannelMode mode)
        {
            if (message is null) return (mode & VoteChannelMode.All) != 0;

            if ((mode & VoteChannelMode.All) != 0) return true;
            if ((mode & VoteChannelMode.Images) != 0 && message.IsImage()) return true;
            if ((mode & VoteChannelMode.Videos) != 0 && message.IsVideo()) return true;
            if ((mode & VoteChannelMode.Music) != 0 && message.IsMusic()) return true;
            if ((mode & VoteChannelMode.Attachments) != 0 && message.IsAttachment()) return true;
            if ((mode & VoteChannelMode.Links) != 0 && message.IsLink()) return true;
            if ((mode & VoteChannelMode.Embeds) != 0 && message.IsEmbed()) return true;

            return false;
        }
    }
}
