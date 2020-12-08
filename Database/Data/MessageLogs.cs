using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class MessageLogs
    {
        public static List<MessageLogsRow> GetRows(ulong? guildId = null, bool ignoreCache = false)
        {
            List<MessageLogsRow> matchedRows = new List<MessageLogsRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.MessageLogs.Rows);

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

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(MessageLogsRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetString(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static MessageLogsRow GetRow(ulong guildId)
        {
            List<MessageLogsRow> rows = GetRows(guildId);
            return rows.Count > 0 ? rows.First() : new MessageLogsRow(guildId);
        }

        public static void SaveRow(MessageLogsRow row)
        {
            MySqlCommand command;

            if (row.New) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO MessageLogs (GuildId, DeletedChannelId, EditedChannelId, ExcludedChannels) VALUES (@GuildId, @DeletedChannelId, @EditedChannelId, @ExcludedChannels);",
                    new [] { ("GuildId", row.GuildId.ToString()), 
                        ("DeletedChannelId", row.DeletedChannelId.ToString()),
                        ("EditedChannelId", row.EditedChannelId.ToString()),
                        ("ExcludedChannels", row.GetExcludedChannelsString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
                row.New = false;
                
                if(Cache.Initialised) Cache.MessageLogs.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE MessageLogs SET DeletedChannelId = @DeletedChannelId, EditedChannelId = @EditedChannelId, ExcludedChannels = @ExcludedChannels WHERE GuildId = @GuildId;",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("DeletedChannelId", row.DeletedChannelId.ToString()),
                        ("EditedChannelId", row.EditedChannelId.ToString()),
                        ("ExcludedChannels", row.GetExcludedChannelsString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.MessageLogs.Rows[Cache.MessageLogs.Rows.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static List<MessageLogsMessageRow> GetMessages(ulong? guildId = null, ulong? channelId = null, ulong? messageId = null)
        {
            List<MessageLogsMessageRow> matchedRows = new List<MessageLogsMessageRow>();

            string command = "SELECT * FROM MessageLogsMessages WHERE TRUE";
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

            if (messageId.HasValue)
            {
                command += " AND MessageId = @MessageId";
                values.Add(("MessageId", messageId.Value.ToString()));
            }

            MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

            while (reader.Read())
            {
                matchedRows.Add(MessageLogsMessageRow.FromDatabase(
                    reader.GetUInt64(0),
                    reader.GetUInt64(1),
                    reader.GetUInt64(2),
                    reader.GetUInt64(3),
                    reader.GetDateTime(4),
                    reader.GetString(5)));
            }

            reader.Close();

            return matchedRows;
        }

        public static List<MessageLogsMessageRow> GetMessages(ulong guildId, ulong channelId, ulong[] messageIds = null)
        {
            List<MessageLogsMessageRow> matchedRows = new List<MessageLogsMessageRow>();

            string command = $"SELECT * FROM MessageLogsMessages WHERE GuildId = @GuildId AND ChannelId = @ChannelId AND MessageId IN {Sql.ToSqlObjectArray(messageIds)}";
            List<(string, string)> values = new List<(string, string)>
            {
                ("GuildId", guildId.ToString()), 
                ("ChannelId", channelId.ToString())
            };

            MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

            while (reader.Read())
            {
                matchedRows.Add(MessageLogsMessageRow.FromDatabase(
                    reader.GetUInt64(0),
                    reader.GetUInt64(1),
                    reader.GetUInt64(2),
                    reader.GetUInt64(3),
                    reader.GetDateTime(4),
                    reader.GetString(5)));
            }

            reader.Close();

            return matchedRows;
        }

        public static MessageLogsMessageRow GetMessage(ulong guildId, ulong channelId, ulong messageId)
        {
            List<MessageLogsMessageRow> messages = GetMessages(guildId, channelId, messageId);

            if (messages.Count == 0)
            {
                return null;
            }

            return messages.First();
        }

        public static void SaveMessage(MessageLogsMessageRow row)
        {
            MySqlCommand command;

            if (row.New) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO MessageLogsMessages (GuildId, ChannelId, MessageId, UserId, Timestamp, Content) VALUES (@GuildId, @ChannelId, @MessageId, @UserId, @Timestamp, @Content);",
                    new [] { ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("MessageId", row.MessageId.ToString()),
                        ("UserId", row.UserId.ToString()),
                        ("Timestamp", Sql.ToSqlDateTime(row.Timestamp)),
                        ("Content", row.Content.GetEncryptedValue(row.Ids))
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
                row.New = false;
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE MessageLogsMessages SET GuildId = @GuildId, ChannelId = @ChannelId, MessageId = @MessageId, UserId = @UserId, Timestamp = @Timestamp, Content = @Content WHERE GuildId = @GuildId AND ChannelId = @ChannelId AND MessageId = @MessageId",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("MessageId", row.MessageId.ToString()),
                        ("UserId", row.UserId.ToString()),
                        ("Timestamp", Sql.ToSqlDateTime(row.Timestamp)),
                        ("Content", row.Content.GetEncryptedValue(row.Ids))
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
            }
        }

        public static void DeleteMessages(ulong guildId, ulong channelId, ulong[] messageIds)
        {
            if(messageIds.Length == 0) return;

            MySqlCommand command =
                Sql.GetCommand($"DELETE FROM MessageLogsMessages WHERE GuildId = @GuildId AND ChannelId = @ChannelId AND MessageId IN {Sql.ToSqlObjectArray(messageIds)};",
                    new [] {
                        ("GuildId", guildId.ToString()), 
                        ("ChannelId", channelId.ToString())
                    });

            command.ExecuteNonQuery();

            command.Connection.Close();
        }

        public static void DeleteOldMessages(ulong guildId, ulong channelId, bool premium)
        {
            if(premium) return;

            List<MessageLogsMessageRow> messages = GetMessages(guildId, channelId).OrderBy(x => x.Timestamp).ToList();
            List<MessageLogsMessageRow> messagesToRemove = new List<MessageLogsMessageRow>();

            messagesToRemove.AddRange(messages.Take(messages.Count - 50));

            DeleteMessages(guildId, channelId, messagesToRemove.Select(x => x.MessageId).ToArray());
        }

        public static void Delete30DayMessages()
        {
            // Deletes messages older than 30 days to comply with Discord's rule for storing message content
            // "The maximum we can permit bots to store encrypted message content is 30 days"
            // Called every 30 seconds from MessageLogsTable.Load

            MySqlCommand command =
                Sql.GetCommand("DELETE FROM MessageLogsMessages WHERE Timestamp <= @MinimumTimestamp;",
                    new []
                    {
                        ("MinimumTimestamp", Sql.ToSqlDateTime(DateTime.UtcNow - TimeSpan.FromDays(30)))
                    });

            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class MessageLogsTable
    {
        public List<MessageLogsRow> Rows { get; set; }
    }

    public class MessageLogsRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong DeletedChannelId { get; set; }
        public ulong EditedChannelId { get; set; }
        public List<ulong> ExcludedChannels { get; set; }

        private MessageLogsRow()
        {

        }

        public MessageLogsRow(ulong guildId)
        {
            New = true;
            GuildId = guildId;
            DeletedChannelId = 0;
            EditedChannelId = 0;
            ExcludedChannels = new List<ulong>();
        }

        public static MessageLogsRow FromDatabase(ulong guildId, ulong deletedChannelId, ulong editedChannelId, string excludedChannels)
        {
            MessageLogsRow row = new MessageLogsRow
            {
                New = false,
                GuildId = guildId,
                DeletedChannelId = deletedChannelId,
                EditedChannelId = editedChannelId,
                ExcludedChannels = new List<ulong>()
            };

            if (!string.IsNullOrEmpty(excludedChannels))
            {
                foreach (string excludedChannel in excludedChannels.Split(","))
                {
                    if (ulong.TryParse(excludedChannel, out ulong channelId))
                    {
                        row.ExcludedChannels.Add(channelId);
                    }
                }
            }

            return row;
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
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public ulong UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public ulong[] Ids => new [] {GuildId, ChannelId, MessageId, UserId};

        public EString Content { get; set; }

        public MessageLogsMessageRow()
        {
            New = true;
        }

        public static MessageLogsMessageRow FromDatabase(ulong guildId, ulong channelId, ulong messageId, ulong userId, DateTime timestamp, string content)
        {
            MessageLogsMessageRow row = new MessageLogsMessageRow
            {
                New = false,
                GuildId = guildId,
                ChannelId = channelId,
                MessageId = messageId,
                UserId = userId,
                Timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc)
            };
            row.Content = EString.FromEncrypted(content, row.Ids);
            return row;
        }
    }
}