using System.Net;
using System.Threading.Tasks;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using static Utili.MessageSender;

namespace Utili.Features
{
    internal static class JoinMessage
    {
        public static async Task UserJoined(SocketGuildUser user)
        {
            SocketGuild guild = user.Guild;
            JoinMessageRow row = await Database.Data.JoinMessage.GetRowAsync(guild.Id);

            if (row.Enabled)
            {
                (JoinMessageRow, string, Embed) joinMessage = GetJoinMessage(row, user);
                if (joinMessage.Item1.Direct)
                {
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(joinMessage.Item2, embed: joinMessage.Item3);
                }
                else
                {
                    ITextChannel channel = guild.GetTextChannel(joinMessage.Item1.ChannelId);
                    if(!channel.BotHasPermissions(ChannelPermission.ViewChannel, ChannelPermission.SendMessages, ChannelPermission.EmbedLinks)) return;

                    await channel.SendMessageAsync(joinMessage.Item2, embed: joinMessage.Item3);
                }
            }
        }

        public static (JoinMessageRow, string, Embed) GetJoinMessage(JoinMessageRow row, SocketGuildUser user)
        {
            (JoinMessageRow, string, Embed) joinMessage = (null, null, null);

            joinMessage.Item1 = row;
            joinMessage.Item2 = row.Text.Value;

            string userAvatarUrl = user.GetAvatarUrl();
            if (string.IsNullOrEmpty(userAvatarUrl)) userAvatarUrl = user.GetDefaultAvatarUrl();

            string iconUrl = row.Icon.Value;
            string thumbnailUrl = row.Thumbnail.Value;
            string imageUrl = row.Image.Value;

            if (iconUrl.ToLower() == "user") iconUrl = userAvatarUrl;
            if (thumbnailUrl.ToLower() == "user") thumbnailUrl = userAvatarUrl;
            if (imageUrl.ToLower() == "user") imageUrl = userAvatarUrl;
            
            if(!IsValidImageUrl(iconUrl)) iconUrl = null;
            if(!IsValidImageUrl(thumbnailUrl)) thumbnailUrl = null;
            if(!IsValidImageUrl(imageUrl)) imageUrl = null;

            EmbedBuilder embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = row.Title.Value.Replace("%user%", user.ToString()),
                    IconUrl = iconUrl
                },
                Description = row.Content.Value.Replace(@"\n", "\n").Replace("%user%", user.Mention),
                Footer = new EmbedFooterBuilder
                {
                    Text = row.Footer.Value.Replace(@"\n", "\n").Replace("%user%", user.ToString())
                },
                ThumbnailUrl = thumbnailUrl,
                ImageUrl = imageUrl,
                Color = row.Colour
            };

            joinMessage.Item3 = embed.Build();

            return joinMessage;
        }

        private static bool IsValidImageUrl(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Timeout = 2000;
                WebResponse response = request.GetResponse();

                if (response.ContentType.ToLower().StartsWith("image/")) return true;
                return false;
            }
            catch
            {
                return false;
            }
        }
    }

    [Group("JoinMessage"), Alias("JoinMessages")]
    public class JoinMessageCommands : ModuleBase<SocketCommandContext>
    {
        [Command("Preview")]
        public async Task Preview()
        {
            JoinMessageRow row = await Database.Data.JoinMessage.GetRowAsync(Context.Guild.Id);
            (JoinMessageRow, string, Embed) joinMessage = JoinMessage.GetJoinMessage(row, Context.User as SocketGuildUser);
            await SendEmbedAsync(Context.Channel, joinMessage.Item3, joinMessage.Item2);
        }
    }
}
