using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    internal static class Notices
    {
        private static List<(NoticesRow, DateTime)> _requiredUpdates = new List<(NoticesRow, DateTime)>();
        private static Timer _timer;

        public static void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(2000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        public static async Task MessageReceived(SocketCommandContext context, SocketMessage partialMessage)
        {
            NoticesRow row = await Database.Data.Notices.GetRowAsync(context.Guild.Id, context.Channel.Id);
            if (!row.Enabled) return;

            TimeSpan delay = row.Delay;
            TimeSpan minimumDelay = context.User.IsBot ? TimeSpan.FromSeconds(10) : TimeSpan.FromSeconds(5);
            if (delay < minimumDelay) delay = minimumDelay;

            lock (_requiredUpdates)
            {
                if (_requiredUpdates.Any(x => x.Item1.ChannelId == context.Channel.Id))
                {
                    (NoticesRow, DateTime) update = _requiredUpdates.First(x => x.Item1.ChannelId == context.Channel.Id);
                    update.Item2 = DateTime.UtcNow + delay;
                    _requiredUpdates.RemoveAll(x => x.Item1.ChannelId == context.Channel.Id);
                    _requiredUpdates.Add(update);
                }
                else
                {
                    if (context.User.Id == _client.CurrentUser.Id) return;
                    _requiredUpdates.Add((row, DateTime.UtcNow + delay));
                }
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                List<MiscRow> fromDashboard = await Misc.GetRowsAsync(type: "RequiresNoticeUpdate");
                foreach (MiscRow miscRow in fromDashboard)
                {
                    await Misc.DeleteRowAsync(miscRow);
                    NoticesRow row = await Database.Data.Notices.GetRowAsync(miscRow.GuildId, ulong.Parse(miscRow.Value));
                    lock (_requiredUpdates)
                    {
                        if (_requiredUpdates.Any(x => x.Item1.ChannelId == row.ChannelId))
                        {
                            (NoticesRow, DateTime) update = _requiredUpdates.First(x => x.Item1.ChannelId == row.ChannelId);
                            update.Item2 = DateTime.MinValue;
                            _requiredUpdates.RemoveAll(x => x.Item1.ChannelId == row.ChannelId);
                            _requiredUpdates.Add(update);
                        }
                        else
                        {
                            _requiredUpdates.Add((row, DateTime.MinValue));
                        }
                    }
                }

                await UpdateNoticesAsync();
            });
            
        }

        private static async Task UpdateNoticesAsync()
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

        private static async Task UpdateNoticeAsync((NoticesRow, DateTime) update)
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

            if (row.Enabled)
            {
                (string, Embed) notice = GetNotice(row);
                RestUserMessage sent = await MessageSender.SendEmbedAsync(channel, notice.Item2, notice.Item1);
                row.MessageId = sent.Id;
                await Database.Data.Notices.SaveMessageIdAsync(row);

                await sent.PinAsync();
            }
        }

        public static (string, Embed) GetNotice(NoticesRow row)
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
                Description = row.Content.Value.Replace(@"\n", "\n"),
                Footer = new EmbedFooterBuilder
                {
                    Text = row.Footer.Value.Replace(@"\n", "\n")
                },
                ThumbnailUrl = thumbnailUrl,
                ImageUrl = imageUrl,
                Color = row.Colour
            };

            return (row.Text.Value, embed.Build());
        }

        private static bool IsValidImageUrl(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Timeout = 2000;
                WebResponse response = request.GetResponse();

                return response.ContentType.ToLower().StartsWith("image/");
            }
            catch
            {
                return false;
            }
        }
    }

    [Group("Notice"), Alias("Notices")]
    public class NoticeCommands : ModuleBase<SocketCommandContext>
    {
        [Command("Preview"), Alias("Send")]
        public async Task Preview(ITextChannel channel = null)
        {
            channel ??= Context.Channel as ITextChannel;

            NoticesRow row = await Database.Data.Notices.GetRowAsync(Context.Guild.Id, channel.Id);
            (string, Embed) notice = Notices.GetNotice(row);
            
            await MessageSender.SendEmbedAsync(Context.Channel, notice.Item2, notice.Item1);
        }
    }
}
