using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class MessageFilter
    {
        public static List<MessageFilterRow> GetRows(ulong? guildId = null, ulong? channelId = null, bool ignoreCache = false)
        {
            List<MessageFilterRow> matchedRows = new List<MessageFilterRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.MessageFilter.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
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

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(MessageFilterRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetInt32(2),
                        reader.GetString(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static void SaveRow(MessageFilterRow row)
        {
            MySqlCommand command;

            if (row.New) 
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

                row.New = false;

                if(Cache.Initialised) Cache.MessageFilter.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE MessageFilter SET Mode = @Mode, Complex = @Complex WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Mode", row.Mode.ToString()),
                        ("Complex", row.Complex.EncodedValue)
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.MessageFilter.Rows[Cache.MessageFilter.Rows.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static void DeleteRow(MessageFilterRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.MessageFilter.Rows.RemoveAll(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId);

            string commandText = "DELETE FROM MessageFilter WHERE GuildId = @GuildId AND ChannelId = @ChannelId";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {
                    ("GuildId", row.GuildId.ToString()),
                    ("ChannelId", row.ChannelId.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class MessageFilterTable
    {
        public List<MessageFilterRow> Rows { get; set; }
    }

    public class MessageFilterRow
    {
        public bool New { get; set; }
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
            New = true;
        }

        public static MessageFilterRow FromDatabase(ulong guildId, ulong channelId, int mode, string complex)
        {
            return new MessageFilterRow
            {
                New = false,
                GuildId = guildId,
                ChannelId = channelId,
                Mode = mode,
                Complex = EString.FromEncoded(complex)
            };
        }
    }
}