using System.Collections.Generic;
using System.Linq;
using Discord;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class JoinMessage
    {
        public static List<JoinMessageRow> GetRows(ulong? guildId = null, bool ignoreCache = false)
        {
            List<JoinMessageRow> matchedRows = new List<JoinMessageRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.JoinMessage.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM JoinMessage WHERE TRUE";
                List<(string, string)> values = new List<(string, string)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(JoinMessageRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetBoolean(1),
                        reader.GetBoolean(2),
                        reader.GetUInt64(3),
                        reader.GetString(4),
                        reader.GetString(5),
                        reader.GetString(6),
                        reader.GetString(7),
                        reader.GetString(8),
                        reader.GetString(9),
                        reader.GetString(10),
                        reader.GetUInt32(11)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static JoinMessageRow GetRow(ulong guildId)
        {
            List<JoinMessageRow> rows = GetRows(guildId);
            return rows.Count > 0 ? rows.First() : new JoinMessageRow(guildId);
        }

        public static void SaveRow(JoinMessageRow row)
        {
            MySqlCommand command;

            if (row.New)
            {
                command = Sql.GetCommand(
                    $"INSERT INTO JoinMessage (GuildId, Enabled, Direct, ChannelId, Title, Footer, Content, Text, Image, Thumbnail, Icon, Colour) VALUES (@GuildId, {Sql.ToSqlBool(row.Enabled)}, {Sql.ToSqlBool(row.Direct)}, @ChannelId, @Title, @Footer, @Content, @Text, @Image, @Thumbnail, @Icon, @Colour);",
                    new[]
                    {
                        ("GuildId", row.GuildId.ToString()),
                        ("ChannelId", row.ChannelId.ToString()),
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

                if(Cache.Initialised) Cache.JoinMessage.Rows.Add(row);
            }
            else
            {
                command = Sql.GetCommand($"UPDATE JoinMessage SET Enabled = {Sql.ToSqlBool(row.Enabled)}, Direct = {Sql.ToSqlBool(row.Direct)}, ChannelId = @ChannelId, Title = @Title, Footer = @Footer, Content = @Content, Text = @Text, Image = @Image, Thumbnail = @Thumbnail, Icon = @Icon, Colour = @Colour WHERE GuildId = @GuildId;",
                    new [] 
                    {
                        ("GuildId", row.GuildId.ToString()),
                        ("ChannelId", row.ChannelId.ToString()),
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

                if(Cache.Initialised) Cache.JoinMessage.Rows[Cache.JoinMessage.Rows.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static void DeleteRow(JoinMessageRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.JoinMessage.Rows.RemoveAll(x => x.GuildId == row.GuildId);

            string commandText = "DELETE FROM JoinMessage WHERE GuildId = @GuildId";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {("GuildId", row.GuildId.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class JoinMessageTable
    {
        public List<JoinMessageRow> Rows { get; set; }
    }

    public class JoinMessageRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public bool Enabled { get; set; }
        public bool Direct { get; set; }
        public ulong ChannelId { get; set; }
        public EString Title { get; set; }
        public EString Footer { get; set; }
        public EString Content { get; set; }
        public EString Text { get; set; }
        public EString Image { get; set; }
        public EString Thumbnail { get; set; }
        public EString Icon { get; set; }
        public Color Colour { get; set; }

        private JoinMessageRow()
        {

        }

        public JoinMessageRow(ulong guildId)
        {
            New = true;
            GuildId = guildId;
            Enabled = false;
            Direct = false;
            ChannelId = 0;
            Title = EString.Empty;
            Footer = EString.Empty;
            Content = EString.Empty;
            Text = EString.Empty;
            Image = EString.Empty;
            Thumbnail = EString.Empty;
            Icon = EString.Empty;
            Colour = new Color(67, 181, 129);
        }

        public static JoinMessageRow FromDatabase(ulong guildId, bool enabled, bool direct, ulong channelId, string title, string footer, string content, string text, string image, string thumbnail, string icon, uint colour)
        {
            return new JoinMessageRow
            {
                New = false,
                GuildId = guildId,
                Enabled = enabled,
                Direct = direct,
                ChannelId = channelId,
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
