using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Data
{
    public static class Notices
    {
        public static async Task<List<NoticesRow>> GetRowsAsync(ulong? guildId = null, ulong? channelId = null, bool ignoreCache = false)
        {
            var matchedRows = new List<NoticesRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Notices);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
            }
            else
            {
                var command = "SELECT * FROM Notices WHERE TRUE";
                var values = new List<(string, object)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                if (channelId.HasValue)
                {
                    command += " AND ChannelId = @ChannelId";
                    values.Add(("ChannelId", channelId.Value));
                }

                var reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(NoticesRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetBoolean(3),
                        reader.GetString(4),
                        reader.GetString(5),
                        reader.GetString(6),
                        reader.GetString(7),
                        reader.GetString(8),
                        reader.GetString(9),
                        reader.GetString(10),
                        reader.GetString(11),
                        reader.GetUInt32(12)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<NoticesRow> GetRowAsync(ulong guildId, ulong channelId)
        {
            var rows = await GetRowsAsync(guildId, channelId);
            return rows.Count > 0 ? rows.First() : new NoticesRow(guildId, channelId);
        }

        public static async Task SaveRowAsync(NoticesRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO Notices (GuildId, ChannelId, MessageId, Enabled, Delay, Title, Footer, Content, Text, Image, Thumbnail, Icon, Colour) VALUES (@GuildId, @ChannelId, @MessageId, @Enabled, @Delay, @Title, @Footer, @Content, @Text, @Image, @Thumbnail, @Icon, @Colour);",
                    ("GuildId", row.GuildId),
                    ("ChannelId", row.ChannelId),
                    ("MessageId", row.MessageId),
                    ("Enabled", row.Enabled),
                    ("Delay", row.Delay),
                    ("Title", row.Title.EncodedValue),
                    ("Footer", row.Footer.EncodedValue),
                    ("Content", row.Content.EncodedValue),
                    ("Text", row.Text.EncodedValue),
                    ("Image", row.Image.EncodedValue),
                    ("Thumbnail", row.Thumbnail.EncodedValue),
                    ("Icon", row.Icon.EncodedValue),
                    ("Colour", row.Colour));

                row.New = false;
                if(Cache.Initialised) Cache.Notices.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE Notices SET MessageId = @MessageId, Enabled = @Enabled, Delay = @Delay, Title = @Title, Footer = @Footer, Content = @Content, Text = @Text, Image = @Image, Thumbnail = @Thumbnail, Icon = @Icon, Colour = @Colour WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    ("GuildId", row.GuildId),
                    ("ChannelId", row.ChannelId),
                    ("MessageId", row.MessageId),
                    ("Enabled", row.Enabled),
                    ("Delay", row.Delay),
                    ("Title", row.Title.EncodedValue),
                    ("Footer", row.Footer.EncodedValue),
                    ("Content", row.Content.EncodedValue),
                    ("Text", row.Text.EncodedValue),
                    ("Image", row.Image.EncodedValue),
                    ("Thumbnail", row.Thumbnail.EncodedValue),
                    ("Icon", row.Icon.EncodedValue),
                    ("Colour", row.Colour));

                if(Cache.Initialised) Cache.Notices[Cache.Notices.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static async Task SaveMessageIdAsync(NoticesRow row)
        {
            if (row.New)
            {
                await SaveRowAsync(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE Notices SET MessageId = @MessageId WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    ("GuildId", row.GuildId),
                    ("ChannelId", row.ChannelId),
                    ("MessageId", row.MessageId));

                if(Cache.Initialised) Cache.Notices[Cache.Notices.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static async Task DeleteRowAsync(NoticesRow row)
        {
            if(Cache.Initialised) Cache.Notices.RemoveAll(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId);

            await Sql.ExecuteAsync(
                "DELETE FROM Notices WHERE GuildId = @GuildId AND ChannelId = @ChannelId",
                ("GuildId", row.GuildId),
                ("ChannelId", row.ChannelId));
        }
    }
    public class NoticesRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public bool Enabled { get; set; }
        public TimeSpan Delay { get; set; }
        public EString Title { get; set; }
        public EString Footer { get; set; }
        public EString Content { get; set; }
        public EString Text { get; set; }
        public EString Image { get; set; }
        public EString Thumbnail { get; set; }
        public EString Icon { get; set; }
        public uint Colour { get; set; }

        private NoticesRow()
        {
            New = true;
        }

        public NoticesRow(ulong guildId, ulong channelId)
        {
            New = true;
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = 0;
            Enabled = false;
            Delay = TimeSpan.FromMinutes(5);
            Title = EString.Empty;
            Footer = EString.Empty;
            Content = EString.Empty;
            Text = EString.Empty;
            Image = EString.Empty;
            Thumbnail = EString.Empty;
            Icon = EString.Empty;
            Colour = 4437377;
        }

        public static NoticesRow FromDatabase(ulong guildId, ulong channelId, ulong messageId, bool enabled, string delay, string title, string footer, string content, string text, string image, string thumbnail, string icon, uint colour)
        {
            return new()
            {
                New = false,
                GuildId = guildId,
                ChannelId = channelId,
                MessageId = messageId,
                Enabled = enabled,
                Delay = TimeSpan.Parse(delay),
                Title = EString.FromEncoded(title),
                Footer = EString.FromEncoded(footer),
                Content = EString.FromEncoded(content),
                Text = EString.FromEncoded(text),
                Image = EString.FromEncoded(image),
                Thumbnail = EString.FromEncoded(thumbnail),
                Icon = EString.FromEncoded(icon),
                Colour = colour
            };
        }

        public async Task SaveAsync()
        {
            await Notices.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await Notices.DeleteRowAsync(this);
        }
    }
}
