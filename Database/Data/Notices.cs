using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Notices
    {
        public static List<NoticesRow> GetRows(ulong? guildId = null, ulong? channelId = null, bool ignoreCache = false)
        {
            List<NoticesRow> matchedRows = new List<NoticesRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Notices.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
            }
            else
            {
                string command = "SELECT * FROM Notices WHERE TRUE";
                List<(string, string)> values = new List<(string, string)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                if (channelId.HasValue)
                {
                    command += " AND ChannelId = @ChannelId";
                    values.Add(("ChannelId", channelId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

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

        public static void SaveRow(NoticesRow row)
        {
            MySqlCommand command;

            if (row.New) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand(
                    $"INSERT INTO Notices (GuildId, ChannelId, MessageId, Enabled, Delay, Title, Footer, Content, Text, Image, Thumbnail, Icon, Colour) VALUES (@GuildId, @ChannelId, @MessageId, {Sql.ToSqlBool(row.Enabled)}, @Delay, @Title, @Footer, @Content, @Text, @Image, @Thumbnail, @Icon, @Colour);",
                    new[]
                    {
                        ("GuildId", row.GuildId.ToString()),
                        ("ChannelId", row.ChannelId.ToString()),
                        ("MessageId", row.MessageId.ToString()),
                        ("Delay", row.Delay.ToString()),
                        ("Title", row.Title.EncodedValue),
                        ("Footer", row.Footer.EncodedValue),
                        ("Content", row.Content.EncodedValue),
                        ("Text", row.Text.EncodedValue),
                        ("Image", row.Image.EncodedValue),
                        ("Thumbnail", row.Thumbnail.EncodedValue),
                        ("Icon", row.Icon.EncodedValue),
                        ("Colour", row.Colour.RawValue.ToString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;

                if(Cache.Initialised) Cache.Notices.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand($"UPDATE Notices SET MessageId = @MessageId, Enabled = {Sql.ToSqlBool(row.Enabled)}, Delay = @Delay, Title = @Title, Footer = @Footer, Content = @Content, Text = @Text, Image = @Image, Thumbnail = @Thumbnail, Icon = @Icon, Colour = @Colour WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    new [] 
                    {
                        ("GuildId", row.GuildId.ToString()),
                        ("ChannelId", row.ChannelId.ToString()),
                        ("MessageId", row.MessageId.ToString()),
                        ("Delay", row.Delay.ToString()),
                        ("Title", row.Title.EncodedValue),
                        ("Footer", row.Footer.EncodedValue),
                        ("Content", row.Content.EncodedValue),
                        ("Text", row.Text.EncodedValue),
                        ("Image", row.Image.EncodedValue),
                        ("Thumbnail", row.Thumbnail.EncodedValue),
                        ("Icon", row.Icon.EncodedValue),
                        ("Colour", row.Colour.RawValue.ToString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.Notices.Rows[Cache.Notices.Rows.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static void SaveMessageId(NoticesRow row)
        {
            MySqlCommand command;

            if (row.New) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand(
                    $"INSERT INTO Notices (GuildId, ChannelId, MessageId, Enabled, Delay, Title, Footer, Content, Text, Image, Thumbnail, Icon, Colour) VALUES (@GuildId, @ChannelId, @MessageId, {Sql.ToSqlBool(row.Enabled)}, @Delay, @Title, @Footer, @Content, @Text, @Image, @Thumbnail, @Icon, @Colour);",
                    new[]
                    {
                        ("GuildId", row.GuildId.ToString()),
                        ("ChannelId", row.ChannelId.ToString()),
                        ("MessageId", row.MessageId.ToString()),
                        ("Delay", row.Delay.ToString()),
                        ("Title", row.Title.EncodedValue),
                        ("Footer", row.Footer.EncodedValue),
                        ("Content", row.Content.EncodedValue),
                        ("Text", row.Text.EncodedValue),
                        ("Image", row.Image.EncodedValue),
                        ("Thumbnail", row.Thumbnail.EncodedValue),
                        ("Icon", row.Icon.EncodedValue),
                        ("Colour", row.Colour.RawValue.ToString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;

                if(Cache.Initialised) Cache.Notices.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand($"UPDATE Notices SET MessageId = @MessageId WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    new [] 
                    {
                        ("GuildId", row.GuildId.ToString()),
                        ("ChannelId", row.ChannelId.ToString()),
                        ("MessageId", row.MessageId.ToString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.Notices.Rows[Cache.Notices.Rows.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static void DeleteRow(NoticesRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.Notices.Rows.RemoveAll(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId);

            string commandText = "DELETE FROM Notices WHERE GuildId = @GuildId AND ChannelId = @ChannelId";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {
                    ("GuildId", row.GuildId.ToString()),
                    ("ChannelId", row.ChannelId.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class NoticesTable
    {
        public List<NoticesRow> Rows { get; set; }
    }

    public class NoticesRow
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
        public Color Colour { get; set; }

        public NoticesRow()
        {
            New = true;
        }

        public static NoticesRow FromDatabase(ulong guildId, ulong channelId, ulong messageId, bool enabled, string delay, string title, string footer, string content, string text, string image, string thumbnail, string icon, uint colour)
        {
            return new NoticesRow
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
                Colour = new Color(colour)
            };
        }
    }
}
