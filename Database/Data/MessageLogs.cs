using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Timers;

namespace Database.Data
{
    public static class MessageLogs
    {
        private static Timer _deletionTimer;

        public static void Initialise()
        {
            _deletionTimer?.Dispose();

            _deletionTimer = new Timer(60000);
            _deletionTimer.Elapsed += DeletionTimer_Elapsed;
            _deletionTimer.Start();
        }

        private static void DeletionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = Delete30DayMessagesAsync();
        }

        public static async Task<List<MessageLogsRow>> GetRowsAsync(ulong? guildId = null, bool ignoreCache = false)
        {
            List<MessageLogsRow> matchedRows = new();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.MessageLogs);
                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM MessageLogs WHERE TRUE";
                List<(string, object)> values = new();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());
                
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

        public static async Task<MessageLogsRow> GetRowAsync(ulong guildId)
        {
            List<MessageLogsRow> rows = await GetRowsAsync(guildId);
            return rows.Count > 0 ? rows.First() : new MessageLogsRow(guildId);
        }

        public static async Task SaveRowAsync(MessageLogsRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO MessageLogs (GuildId, DeletedChannelId, EditedChannelId, ExcludedChannels) VALUES (@GuildId, @DeletedChannelId, @EditedChannelId, @ExcludedChannels);",
                    ("GuildId", row.GuildId),
                    ("DeletedChannelId", row.DeletedChannelId),
                    ("EditedChannelId", row.EditedChannelId),
                    ("ExcludedChannels", row.GetExcludedChannelsString()));

                row.New = false;
                if(Cache.Initialised) Cache.MessageLogs.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE MessageLogs SET DeletedChannelId = @DeletedChannelId, EditedChannelId = @EditedChannelId, ExcludedChannels = @ExcludedChannels WHERE GuildId = @GuildId;",
                    ("GuildId", row.GuildId), 
                    ("DeletedChannelId", row.DeletedChannelId),
                    ("EditedChannelId", row.EditedChannelId),
                    ("ExcludedChannels", row.GetExcludedChannelsString()));

                if(Cache.Initialised) Cache.MessageLogs[Cache.MessageLogs.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static async Task DeleteRowAsync(MessageLogsRow row)
        {
            if(Cache.Initialised) Cache.MessageLogs.RemoveAll(x => x.GuildId == row.GuildId);

            await Sql.ExecuteAsync(
                "DELETE FROM Autopurge WHERE GuildId = @GuildId",
                ("GuildId", row.GuildId));
        }

        public static async Task<List<MessageLogsMessageRow>> GetMessagesAsync(ulong? guildId = null, ulong? channelId = null, ulong? messageId = null)
        {
            List<MessageLogsMessageRow> matchedRows = new();

            string command = "SELECT * FROM MessageLogsMessages WHERE TRUE";
            List<(string, object)> values = new();

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

            if (messageId.HasValue)
            {
                command += " AND MessageId = @MessageId";
                values.Add(("MessageId", messageId.Value));
            }

            MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

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

        public static async Task<List<MessageLogsMessageRow>> GetMessagesAsync(ulong guildId, ulong channelId, ulong[] messageIds)
        {
            List<MessageLogsMessageRow> matchedRows = new();

            string command = $"SELECT * FROM MessageLogsMessages WHERE GuildId = @GuildId AND ChannelId = @ChannelId AND MessageId IN {Sql.ToSqlObjectArray(messageIds)}";
            List<(string, object)> values = new()
            {
                ("GuildId", guildId), 
                ("ChannelId", channelId)
            };

            MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

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

        public static async Task<MessageLogsMessageRow> GetMessageAsync(ulong guildId, ulong channelId, ulong messageId)
        {
            List<MessageLogsMessageRow> messages = await GetMessagesAsync(guildId, channelId, messageId);
            return messages.Count > 0 ? messages.First() : null;
        }

        public static async Task SaveMessageAsync(MessageLogsMessageRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync("INSERT INTO MessageLogsMessages (GuildId, ChannelId, MessageId, UserId, Timestamp, Content) VALUES (@GuildId, @ChannelId, @MessageId, @UserId, @Timestamp, @Content);",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("MessageId", row.MessageId),
                    ("UserId", row.UserId),
                    ("Timestamp", row.Timestamp),
                    ("Content", row.Content.GetEncryptedValue(row.Ids)));

                row.New = false;
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE MessageLogsMessages SET GuildId = @GuildId, ChannelId = @ChannelId, MessageId = @MessageId, UserId = @UserId, Timestamp = @Timestamp, Content = @Content WHERE GuildId = @GuildId AND ChannelId = @ChannelId AND MessageId = @MessageId",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("MessageId", row.MessageId),
                    ("UserId", row.UserId),
                    ("Timestamp", row.Timestamp),
                    ("Content", row.Content.GetEncryptedValue(row.Ids)));
            }
        }

        public static async Task DeleteMessagesAsync(ulong guildId, ulong channelId, ulong[] messageIds)
        {
            if(messageIds.Length == 0) return;
            await Sql.ExecuteAsync(
                $"DELETE FROM MessageLogsMessages WHERE GuildId = @GuildId AND ChannelId = @ChannelId AND MessageId IN {Sql.ToSqlObjectArray(messageIds)};",
                ("GuildId", guildId), 
                ("ChannelId", channelId));
        }

        private static async Task<int> GetStoredMessageCountAsync(ulong guildId, ulong channelId)
        {
            string command = "SELECT COUNT(*) FROM MessageLogsMessages WHERE GuildId = @GuildId AND ChannelId = @ChannelId";
            List<(string, object)> values = new()
            {
                ("GuildId", guildId), 
                ("ChannelId", channelId)
            };

            MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());
            reader.Read();
            int count = reader.GetInt32(0);
            reader.Close();
            return count;
        }

        public static async Task DeleteOldMessagesAsync(ulong guildId, ulong channelId, bool premium)
        {
            if(premium) return;

            int count = await GetStoredMessageCountAsync(guildId, channelId);
            int toDelete = count - 50;
            if(toDelete <= 0) return;

            // Double-nesting is used to workaround a limitation of mysql
            await Sql.ExecuteAsync("DELETE FROM MessageLogsMessages WHERE MessageId IN " +
                                        "(SELECT MessageId FROM (" +
                                            $"SELECT MessageId FROM MessageLogsMessages WHERE GuildId = @GuildId AND ChannelId = @ChannelId ORDER BY `Timestamp` LIMIT {toDelete}" +
                                        ") a" +
                                    ")", 
                ("GuildId", guildId), 
                ("ChannelId", channelId));
        }

        public static async Task Delete30DayMessagesAsync()
        {
            // Deletes messages older than 30 days to comply with Discord's rule for storing message content
            // "The maximum we can permit bots to store encrypted message content is 30 days"

            await Sql.ExecuteAsync(
                "DELETE FROM MessageLogsMessages WHERE Timestamp <= @MinimumTimestamp;",
                ("MinimumTimestamp", DateTime.UtcNow - TimeSpan.FromDays(30)));
        }
    }
    public class MessageLogsRow : IRow
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
            MessageLogsRow row = new()
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

        public async Task SaveAsync()
        {
            await MessageLogs.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await MessageLogs.DeleteRowAsync(this);
        }
    }

    public class MessageLogsMessageRow : IRow
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
            MessageLogsMessageRow row = new()
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

        public async Task SaveAsync()
        {
            await MessageLogs.SaveMessageAsync(this);
        }

        public async Task DeleteAsync()
        {
            await MessageLogs.DeleteMessagesAsync(GuildId, ChannelId, new[] { MessageId });
        }
    }
}