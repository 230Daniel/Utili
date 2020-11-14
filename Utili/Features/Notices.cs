using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Database.Data;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili.Features
{
    internal class Notices
    {
        private List<(NoticesRow, DateTime)> _requiredUpdates = new List<(NoticesRow, DateTime)>();
        private Timer _timer;

        public void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(2000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public async Task MessageReceived(SocketCommandContext context)
        {
            List<NoticesRow> rows = Database.Data.Notices.GetRows(context.Guild.Id, context.Channel.Id);
            if(rows.Count == 0) return;
            NoticesRow row = rows.First();
            if (!row.Enabled) return;

            if (_requiredUpdates.Count(x => x.Item1.ChannelId == context.Channel.Id) > 0)
            {
                (NoticesRow, DateTime) update = _requiredUpdates.First(x => x.Item1.ChannelId == context.Channel.Id);
                update.Item2 = DateTime.UtcNow + row.Delay;
            }
            else
            {
                _requiredUpdates.Add((row, DateTime.UtcNow + row.Delay));
            }
        }

        private bool _updating;
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(_updating) return;

            _ = UpdateNotices();
        }

        private async Task UpdateNotices()
        {
            _updating = true;

            foreach ((NoticesRow, DateTime) update in _requiredUpdates.Where(x => x.Item2 <= DateTime.UtcNow))
            {
                try
                {
                    NoticesRow row = update.Item1;
                    _requiredUpdates.RemoveAll(x => x.Item1.ChannelId == row.ChannelId);

                    SocketGuild guild = _client.GetGuild(row.GuildId);
                    SocketTextChannel channel = guild.GetTextChannel(row.ChannelId);

                    if(BotPermissions.IsMissingPermissions(channel, new [] { ChannelPermission.ViewChannel, ChannelPermission.ReadMessageHistory, ChannelPermission.ManageMessages }, out _)) return;

                    IMessage message = await channel.GetMessageAsync(row.MessageId);

                    await message.DeleteAsync();

                    (string, Embed) notice = GetNotice(row);
                    RestUserMessage sent = await MessageSender.SendEmbedAsync(channel, notice.Item2, notice.Item1);
                    row.MessageId = sent.Id;
                    Database.Data.Notices.SaveMessageId(row);

                    await sent.PinAsync();
                }
                catch {}
            }

            _updating = false;
        }

        public (string, Embed) GetNotice(NoticesRow row)
        {
            string iconUrl = row.Icon.Value;
            string thumbnailUrl = row.Thumbnail.Value;
            string imageUrl = row.Image.Value;

            if(!IsValidImageUrl(iconUrl)) iconUrl = null;
            if(!IsValidImageUrl(thumbnailUrl)) thumbnailUrl = null;
            if(!IsValidImageUrl(imageUrl)) imageUrl = null;

            EmbedBuilder embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = row.Title.Value,
                    IconUrl = iconUrl
                },
                Description = row.Content.Value,
                Footer = new EmbedFooterBuilder
                {
                    Text = row.Footer.Value
                },
                ThumbnailUrl = thumbnailUrl,
                ImageUrl = imageUrl,
                Color = row.Colour
            };

            return (row.Text.Value, embed.Build());
        }

        private bool IsValidImageUrl(string url)
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
}
