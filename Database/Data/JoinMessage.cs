using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public class JoinMessage
    {
        public static List<JoinMessageRow> GetRows(ulong? guildId = null, int? id = null, bool ignoreCache = false)
        {
            List<JoinMessageRow> matchedRows = new List<JoinMessageRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.JoinMessage.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (id.HasValue) matchedRows.RemoveAll(x => x.Id != id.Value);
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

                if (id.HasValue)
                {
                    command += " AND Id = @Id";
                    values.Add(("Id", id.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(new JoinMessageRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
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

        public static void SaveRow(JoinMessageRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand(
                    $"INSERT INTO JoinMessage (GuildId, Direct, ChannelId, Title, Footer, Content, Text, Image, Thumbnail, Icon, Colour) VALUES (@GuildId, {Sql.ToSqlBool(row.Direct)}, @ChannelId, @Title, @Footer, @Content, @Text, @Image, @Thumbnail, @Icon, @Colour);",
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

                row.Id = GetRows(row.GuildId, ignoreCache: true).First().Id;

                if(Cache.Initialised) Cache.JoinMessage.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand($"UPDATE JoinMessage SET GuildId = @GuildId, Direct = {Sql.ToSqlBool(row.Direct)}, ChannelId = @ChannelId, Title = @Title, Footer = @Footer, Content = @Content, Text = @Text, Image = @Image, Thumbnail = @Thumbnail, Icon = @Icon, Colour = @Colour WHERE Id = @Id;",
                    new [] 
                    {
                        ("Id", row.Id.ToString()),
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

                if(Cache.Initialised) Cache.JoinMessage.Rows[Cache.JoinMessage.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void DeleteRow(JoinMessageRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.JoinMessage.Rows.RemoveAll(x => x.Id == row.Id);

            string commandText = "DELETE FROM JoinMessage WHERE Id = @Id";
            MySqlCommand command = Sql.GetCommand(commandText, new[] {("Id", row.Id.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class JoinMessageTable
    {
        public List<JoinMessageRow> Rows { get; set; }

        public void Load()
        // Load the table from the database
        {
            List<JoinMessageRow> newRows = new List<JoinMessageRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM JoinMessage;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new JoinMessageRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
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
            }
            catch {}

            reader.Close();

            Rows = newRows;
        }
    }

    public class JoinMessageRow
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
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

        public JoinMessageRow()
        {
            Id = 0;
        }

        public JoinMessageRow(int id, ulong guildId, bool direct, ulong channelId, string title, string footer, string content, string text, string image, string thumbnail, string icon, uint colour)
        {
            Id = id;
            GuildId = guildId;
            Direct = direct;
            ChannelId = channelId;
            Title = EString.FromEncoded(title);
            Footer = EString.FromEncoded(footer);
            Content = EString.FromEncoded(content);
            Text = EString.FromEncoded(text);
            Image = EString.FromEncoded(image);
            Thumbnail = EString.FromEncoded(thumbnail);
            Icon = EString.FromEncoded(icon);
            Colour = new Color(colour);
        }
    }
}
