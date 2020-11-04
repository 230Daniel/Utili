using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Database.Data
{
    public class MessageLogs
    {
        public static List<MessageLogsRow> GetRows(ulong? guildId = null, int? id = null, bool ignoreCache = false)
        {
            List<MessageLogsRow> matchedRows = new List<MessageLogsRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.MessageLogs.Rows);

                if (id.HasValue) matchedRows.RemoveAll(x => x.Id != id.Value);
                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM MessageLogs WHERE TRUE";
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
                    matchedRows.Add(new MessageLogsRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3),
                        reader.GetString(4)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static void SaveRow(MessageLogsRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand($"INSERT INTO MessageLogs (GuildID, DeletedChannelId, EditedChannelId, ExcludedChannels) VALUES (@GuildId, @DeletedChannelId, @EditedChannelId, @ExcludedChannels);",
                    new [] { ("GuildId", row.GuildId.ToString()), 
                        ("DeletedChannelId", row.DeletedChannelId.ToString()),
                        ("EditedChannelId", row.EditedChannelId.ToString()),
                        ("ExcludedChannels", row.GetExcludedChannelsString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
                row.Id = GetRows(row.GuildId, null, true).First().Id;
                
                if(Cache.Initialised) Cache.MessageLogs.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand($"UPDATE MessageLogs SET GuildId = @GuildId, DeletedChannelId = @DeletedChannelId, EditedChannelId = @EditedChannelId, ExcludedChannels = @ExcludedChannels WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("DeletedChannelId", row.DeletedChannelId.ToString()),
                        ("EditedChannelId", row.EditedChannelId.ToString()),
                        ("ExcludedChannels", row.GetExcludedChannelsString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.MessageLogs.Rows[Cache.MessageLogs.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static List<MessageLogsMessageRow> GetMessages(ulong? guildId = null, ulong? messageId = null, int? id = null)
        {
            List<MessageLogsMessageRow> matchedRows = new List<MessageLogsMessageRow>();

            string command = "SELECT * FROM MessageLogsMessages WHERE TRUE";
            List<(string, string)> values = new List<(string, string)>();

            if (guildId.HasValue)
            {
                command += " AND GuildId = @GuildId";
                values.Add(("GuildId", guildId.Value.ToString()));
            }

            if (messageId.HasValue)
            {
                command += " AND MessageId = @MessageId";
                values.Add(("MessageId", messageId.Value.ToString()));
            }

            if (id.HasValue)
            {
                command += " AND Id = @Id";
                values.Add(("Id", id.Value.ToString()));
            }

            MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

            while (reader.Read())
            {
                matchedRows.Add(new MessageLogsMessageRow(
                    reader.GetInt32(0),
                    reader.GetUInt64(1),
                    reader.GetUInt64(2),
                    reader.GetUInt64(3),
                    reader.GetUInt64(4),
                    reader.GetDateTime(5),
                    reader.GetString(6)));
            }

            reader.Close();

            return matchedRows;
        }

        public static void SaveMessage(MessageLogsMessageRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand($"INSERT INTO MessageLogsMessages (GuildID, ChannelId, MessageId, UserId, Timestamp, Content) VALUES (@GuildId, @ChannelId, @MessageId, @UserId, @Timestamp, @Content);",
                    new [] { ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("MessageId", row.MessageId.ToString()),
                        ("UserId", row.UserId.ToString()),
                        ("Timestamp", Sql.ConvertToSqlTime(row.Timestamp)),
                        ("Content", row.Content)
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
                row.Id = GetMessages(row.GuildId, row.MessageId).First().Id;
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand($"UPDATE MessageLogsMessages SET GuildId = @GuildId, ChannelId = @ChannelId, MessageId = @MessageId, UserId = @UserId, Timestamp = @Timestamp, Content = @Content WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("MessageId", row.MessageId.ToString()),
                        ("UserId", row.UserId.ToString()),
                        ("Timestamp", Sql.ConvertToSqlTime(row.Timestamp)),
                        ("Content", row.Content)
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
            }
        }
    }

    public class MessageLogsTable
    {
        public List<MessageLogsRow> Rows { get; set; }

        public void Load()
        // Load the table from the database
        {
            List<MessageLogsRow> newRows = new List<MessageLogsRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM MessageLogs;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new MessageLogsRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3),
                        reader.GetString(4)));
                }
            }
            catch {}

            reader.Close();
            Rows = newRows;
        }
    }

    public class MessageLogsRow
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong DeletedChannelId { get; set; }
        public ulong EditedChannelId { get; set; }
        public List<ulong> ExcludedChannels { get; set; } = new List<ulong>();

        public MessageLogsRow()
        {
            Id = 0;
        }

        public MessageLogsRow(int id, ulong guildId, ulong deletedChannelId, ulong editedChannelId, string excludedChannels)
        {
            Id = id;
            GuildId = guildId;
            DeletedChannelId = deletedChannelId;
            EditedChannelId = editedChannelId;

            ExcludedChannels.Clear();
            foreach (string excludedChannel in excludedChannels.Split(","))
            {
                if (ulong.TryParse(excludedChannel, out ulong channelId))
                {
                    ExcludedChannels.Add(channelId);
                }
            }
        }

        public string GetExcludedChannelsString()
        {
            string excludedChannelsString = "";

            for (int i = 0; i < ExcludedChannels.Count; i++)
            {
                ulong excludedChannelId = ExcludedChannels[i];
                excludedChannelsString += excludedChannelId.ToString();
                if (i != ExcludedChannels.Count - 1)
                {
                    excludedChannelsString += ",";
                }
            }

            return excludedChannelsString;
        }
    }

    public class MessageLogsMessageRow
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public ulong UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Content { get; set; }

        public MessageLogsMessageRow(int id, ulong guildId, ulong channelId, ulong messageId, ulong userId, DateTime timestamp, string content)
        {
            Id = id;
            GuildId = guildId;
            ChannelId = channelId;
            MessageId = messageId;
            UserId = userId;
            Timestamp = timestamp;
            Content = content;
        }
    }
}