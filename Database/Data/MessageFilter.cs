using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public class MessageFilter
    {
        public static List<MessageFilterRow> GetRows(ulong? guildId = null, ulong? channelId = null, int? id = null, bool ignoreCache = false)
        {
            List<MessageFilterRow> matchedRows = new List<MessageFilterRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.MessageFilter.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
                if (id.HasValue) matchedRows.RemoveAll(x => x.Id != id.Value);
            }
            else
            {
                string command = "SELECT * FROM MessageFilter WHERE TRUE";
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

                if (id.HasValue)
                {
                    command += " AND Id = @Id";
                    values.Add(("Id", id.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(new MessageFilterRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetInt32(3),
                        reader.GetString(4)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static void SaveRow(MessageFilterRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO MessageFilter (GuildId, ChannelId, Mode, Complex) VALUES (@GuildId, @ChannelId, @Mode, @Complex);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Mode", row.Mode.ToString()),
                        ("Complex", row.Complex.EncodedValue)
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.Id = GetRows(row.GuildId, row.ChannelId, ignoreCache: true).First().Id;

                if(Cache.Initialised) Cache.MessageFilter.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE MessageFilter SET GuildId = @GuildId, ChannelId = @ChannelId, Mode = @Mode, Complex = @Complex WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Mode", row.Mode.ToString()),
                        ("Complex", row.Complex.EncodedValue)
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.MessageFilter.Rows[Cache.MessageFilter.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void DeleteRow(MessageFilterRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.MessageFilter.Rows.RemoveAll(x => x.Id == row.Id);

            string commandText = "DELETE FROM MessageFilter WHERE Id = @Id";
            MySqlCommand command = Sql.GetCommand(commandText, new[] {("Id", row.Id.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class MessageFilterTable
    {
        public List<MessageFilterRow> Rows { get; set; }

        public void Load()
        // Load the table from the database
        {
            List<MessageFilterRow> newRows = new List<MessageFilterRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM MessageFilter;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new MessageFilterRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetInt32(3),
                        reader.GetString(4)));
                }
            }
            catch {}

            reader.Close();

            Rows = newRows;
        }
    }

    public class MessageFilterRow
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public int Mode { get; set; }

        // 0    All messages
        // 1    Images
        // 2    Videos
        // 3    Media
        // 4    Music
        // 5    Attachments
        // 6    URLs
        // 7    RegEx

        public EString Complex { get; set; }

        public MessageFilterRow()
        {
            Id = 0;
        }

        public MessageFilterRow(int id, ulong guildId, ulong channelId, int mode, string complex)
        {
            Id = id;
            GuildId = guildId;
            ChannelId = channelId;
            Mode = mode;
            Complex = EString.FromEncoded(complex);
        }
    }
}