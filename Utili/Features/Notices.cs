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

        public async Task MessageReceived(SocketCommandContext context, SocketMessage partialMessage)
        {
            List<NoticesRow> rows = Database.Data.Notices.GetRows(context.Guild.Id, context.Channel.Id);
            if(rows.Count == 0) return;
            NoticesRow row = rows.First();
            if (!row.Enabled) return;

            lock (_requiredUpdates)
            {
                if (_requiredUpdates.Any(x => x.Item1.ChannelId == context.Channel.Id))
                {
                    (NoticesRow, DateTime) update = _requiredUpdates.First(x => x.Item1.ChannelId == context.Channel.Id);
                    update.Item2 = DateTime.UtcNow + row.Delay;
                    _requiredUpdates.RemoveAll(x => x.Item1.ChannelId == context.Channel.Id);
                    _requiredUpdates.Add(update);
                }
                else
                {
                    if(context.User.IsBot) return;
                    _requiredUpdates.Add((row, DateTime.UtcNow + row.Delay));
                }
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateNoticesAsync().GetAwaiter().GetResult();
        }

        private async Task UpdateNoticesAsync()
        {
            List<(NoticesRow, DateTime)> updates = new List<(NoticesRow, DateTime)>();

            lock (_requiredUpdates)
            {
                updates.AddRange(_requiredUpdates.Where(x => x.Item2 <= DateTime.UtcNow));
                _requiredUpdates.RemoveAll(x => x.Item2 <= DateTime.UtcNow);
            }

            List<Task> tasks = updates.Select(UpdateNoticeAsync).ToList();
            await Task.WhenAll(tasks);
        }

        private async Task UpdateNoticeAsync((NoticesRow, DateTime) update)
        {
            await Task.Delay(1);

            NoticesRow row = update.Item1;

            SocketGuild guild = _client.GetGuild(row.GuildId);
            SocketTextChannel channel = guild.GetTextChannel(row.ChannelId);

            if(BotPermissions.IsMissingPermissions(channel, new [] { ChannelPermission.ViewChannel, ChannelPermission.ReadMessageHistory, ChannelPermission.ManageMessages }, out _)) return;

            try
            {
                IMessage message = await channel.GetMessageAsync(row.MessageId);
                await message.DeleteAsync();
            }
            catch {}

            (string, Embed) notice = GetNotice(row);
            RestUserMessage sent = await MessageSender.SendEmbedAsync(channel, notice.Item2, notice.Item1);
            row.MessageId = sent.Id;
            Database.Data.Notices.SaveMessageId(row);

            await sent.PinAsync();
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
