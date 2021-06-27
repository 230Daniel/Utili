using System;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Gateway;
using Disqord.Rest;
using Microsoft.Extensions.Logging;
using Utili.Extensions;

namespace Utili.Services
{
    public class JoinMessageService
    {
        private readonly ILogger<JoinMessageService> _logger;
        private readonly DiscordClientBase _client;

        public JoinMessageService(ILogger<JoinMessageService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task MemberJoined(MemberJoinedEventArgs e)
        {
            try
            {
                var row = await JoinMessage.GetRowAsync(e.GuildId);

                if (row.Enabled)
                {
                    var message = GetJoinMessage(row, e.Member);
                    if (row.Direct) try { await e.Member.SendMessageAsync(message); } catch { }
                    else
                    {
                        ITextChannel channel = _client.GetTextChannel(e.GuildId, row.ChannelId);
                        if(!channel.BotHasPermissions(Permission.ViewChannel | Permission.SendMessages | Permission.EmbedLinks)) return;
                        await channel.SendMessageAsync(message);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception thrown on member joined");
            }
        }

        public static LocalMessage GetJoinMessage(JoinMessageRow row, IMember member)
        {
            var text = row.Text.Value.Replace(@"\n", "\n").Replace("%user%", member.Mention);
            var title = row.Title.Value.Replace("%user%", member.ToString());
            var content = row.Content.Value.Replace(@"\n", "\n").Replace("%user%", member.Mention);
            var footer = row.Footer.Value.Replace(@"\n", "\n").Replace("%user%", member.ToString());
            
            var iconUrl = row.Icon.Value;
            var thumbnailUrl = row.Thumbnail.Value;
            var imageUrl = row.Image.Value;

            if (iconUrl.ToLower() == "user") iconUrl = member.GetAvatarUrl();
            if (thumbnailUrl.ToLower() == "user") thumbnailUrl = member.GetAvatarUrl();
            if (imageUrl.ToLower() == "user") imageUrl = member.GetAvatarUrl();

            if (string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(iconUrl))
                title = "Title";
            
            if (string.IsNullOrWhiteSpace(title) &&
                string.IsNullOrWhiteSpace(content) &&
                string.IsNullOrWhiteSpace(footer) &&
                string.IsNullOrWhiteSpace(iconUrl) &&
                string.IsNullOrWhiteSpace(thumbnailUrl) &&
                string.IsNullOrWhiteSpace(imageUrl))
            {
                return new LocalMessage()
                    .WithRequiredContent(text);
            }

            return new LocalMessage()
                .WithOptionalContent(text)
                .AddEmbed(new LocalEmbed()
                    .WithOptionalAuthor(title, iconUrl)
                    .WithDescription(content)
                    .WithOptionalFooter(footer)
                    .WithThumbnailUrl(thumbnailUrl)
                    .WithImageUrl(imageUrl)
                    .WithColor(new Color((int) row.Colour)));
        }
    }
}
