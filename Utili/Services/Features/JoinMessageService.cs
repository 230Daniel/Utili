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
        ILogger<JoinMessageService> _logger;
        DiscordClientBase _client;

        public JoinMessageService(ILogger<JoinMessageService> logger, DiscordClientBase client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task MemberJoined(object sender, MemberJoinedEventArgs e)
        {
            try
            {
                JoinMessageRow row = await JoinMessage.GetRowAsync(e.GuildId);

                if (row.Enabled)
                {
                    LocalMessage message = GetJoinMessage(row, e.Member);
                    if (row.Direct) try { await e.Member.SendMessageAsync(message); } catch { }
                    else
                    {
                        ITextChannel channel = _client.GetTextChannel(e.GuildId, row.ChannelId);
                        if(!channel.BotHasPermissions(_client, Permission.ViewChannel, Permission.SendMessages, Permission.EmbedLinks)) return;
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
            string text = row.Text.Value.Replace(@"\n", "\n").Replace("%user%", member.Mention);
            string title = row.Title.Value.Replace("%user%", member.ToString());
            string content = row.Content.Value.Replace(@"\n", "\n").Replace("%user%", member.Mention);
            string footer = row.Footer.Value.Replace(@"\n", "\n").Replace("%user%", member.ToString());

            
            string iconUrl = row.Icon.Value;
            string thumbnailUrl = row.Thumbnail.Value;
            string imageUrl = row.Image.Value;

            if (iconUrl.ToLower() == "user") iconUrl = member.GetAvatarUrl();
            if (thumbnailUrl.ToLower() == "user") thumbnailUrl = member.GetAvatarUrl();
            if (imageUrl.ToLower() == "user") imageUrl = member.GetAvatarUrl();

            if (string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(imageUrl))
            {
                title = "Title";
            }

            return new LocalMessageBuilder()
                .WithOptionalContent(text)
                .WithEmbed(new LocalEmbedBuilder()
                    .WithOptionalAuthor(title, iconUrl)
                    .WithDescription(content)
                    .WithOptionalFooter(footer)
                    .WithThumbnailUrl(thumbnailUrl)
                    .WithImageUrl(imageUrl)
                    .WithColor(new Color((int) row.Colour.RawValue)))
                .Build();
        }
    }
}
