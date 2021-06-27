using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Data
{
    public static class Autopurge
    {
        public static async Task<List<AutopurgeRow>> GetRowsAsync(ulong? guildId = null, ulong? channelId = null, bool enabledOnly = false, bool ignoreCache = false)
        {
            var matchedRows = new List<AutopurgeRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Autopurge);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
            }
            else
            {
                var command = "SELECT * FROM Autopurge WHERE TRUE";
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

                if (enabledOnly)
                {
                    command += " AND Mode != @Mode";
                    values.Add(("Mode", 2));
                }

                var reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(AutopurgeRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetString(2),
                        reader.GetInt32(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<AutopurgeRow> GetRowAsync(ulong guildId, ulong channelId)
        {
            var rows = await GetRowsAsync(guildId, channelId);
            return rows.Count > 0 ? rows.First() : new AutopurgeRow(guildId, channelId);
        }

        public static async Task SaveRowAsync(AutopurgeRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync("INSERT INTO Autopurge (GuildId, ChannelId, Timespan, Mode) VALUES (@GuildId, @ChannelId, @Timespan, @Mode);",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("Timespan", row.Timespan),
                    ("Mode", row.Mode));

                row.New = false;

                if(Cache.Initialised) Cache.Autopurge.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE Autopurge SET Timespan = @Timespan, Mode = @Mode WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("Timespan", row.Timespan),
                    ("Mode", row.Mode));

                if(Cache.Initialised) Cache.Autopurge[Cache.Autopurge.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static async Task DeleteRowAsync(AutopurgeRow row)
        {
            if(Cache.Initialised) Cache.Autopurge.RemoveAll(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId);

            await Sql.ExecuteAsync(
                "DELETE FROM Autopurge WHERE GuildId = @GuildId AND ChannelId = @ChannelId",
                ("GuildId", row.GuildId),
                ("ChannelId", row.ChannelId));
        }

        public static async Task<List<AutopurgeMessageRow>> GetMessagesAsync(ulong? guildId = null,
            ulong? channelId = null, ulong? messageId = null)
        {
            var matchedRows = new List<AutopurgeMessageRow>();

            var command = "SELECT * FROM AutopurgeMessages WHERE TRUE";
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

            if (messageId.HasValue)
            {
                command += " AND MessageId = @MessageId";
                values.Add(("MessageId", messageId.Value));
            }

            var reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

            while (reader.Read())
            {
                matchedRows.Add(AutopurgeMessageRow.FromDatabase(
                    reader.GetUInt64(0),
                    reader.GetUInt64(1),
                    reader.GetUInt64(2),
                    reader.GetDateTime(3),
                    reader.GetBoolean(4),
                    reader.GetBoolean(5)));
            }

            reader.Close();
            return matchedRows;
        }

        public static async Task<List<AutopurgeMessageRow>> GetAndDeleteDueMessagesAsync(AutopurgeRow row)
        {
            var matchedRows = new List<AutopurgeMessageRow>();
            if (row.Mode == 2) return matchedRows;

            var reader = await Sql.ExecuteReaderAsync(
                "DELETE FROM AutopurgeMessages WHERE " +
                "GuildId = @GuildId AND " +
                "ChannelId = @ChannelId AND " +
                "Timestamp <= @MaximumTimestamp AND " +
                "Timestamp >= @MinimumTimestamp AND " +
                "(@AllMessages OR IsBot = @IsBot) AND " +
                "IsPinned = @IsPinned " +
                "RETURNING *",

                ("GuildId", row.GuildId),
                ("ChannelId", row.ChannelId),
                ("MaximumTimestamp", DateTime.UtcNow - row.Timespan),
                ("MinimumTimestamp", DateTime.UtcNow - TimeSpan.FromDays(13.9)),
                ("IsBot", row.Mode == 1),
                ("AllMessages", row.Mode == 0),
                ("IsPinned", false));

            while (reader.Read())
            {
                matchedRows.Add(AutopurgeMessageRow.FromDatabase(
                    reader.GetUInt64(0),
                    reader.GetUInt64(1),
                    reader.GetUInt64(2),
                    reader.GetDateTime(3),
                    reader.GetBoolean(4),
                    reader.GetBoolean(5)));
            }

            reader.Close();
            return matchedRows;
        }

        public static async Task SaveMessageAsync(AutopurgeMessageRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync("INSERT INTO AutopurgeMessages (GuildId, ChannelId, MessageId, Timestamp, IsBot, IsPinned) VALUES (@GuildId, @ChannelId, @MessageId, @Timestamp, @IsBot, @IsPinned);",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("MessageId", row.MessageId),
                    ("Timestamp", row.Timestamp),
                    ("IsBot", row.IsBot),
                    ("IsPinned", row.IsPinned));

                row.New = false;
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE AutopurgeMessages SET GuildId = @GuildId, ChannelId = @ChannelId, MessageId = @MessageId, Timestamp = @Timestamp, IsBot = @IsBot, IsPinned = @IsPinned WHERE GuildId = @GuildId AND ChannelId = @ChannelId AND MessageId = @MessageId",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("MessageId", row.MessageId),
                    ("Timestamp", row.Timestamp),
                    ("IsBot", row.IsBot),
                    ("IsPinned", row.IsPinned));
            }
        }
        
        public static async Task DeleteMessageAsync(AutopurgeMessageRow row)
        {
            await Sql.ExecuteAsync(
                "DELETE FROM AutopurgeMessages WHERE GuildId = @GuildId AND ChannelId = @ChannelId AND MessageId = @MessageId",
                ("GuildId", row.GuildId),
                ("ChannelId", row.ChannelId),
                ("MessageId", row.MessageId));
        }
        
        public static async Task DeleteMessagesAsync(AutopurgeRow row, ulong[] messageIds)
        {
            await Sql.ExecuteAsync(
                $"DELETE FROM AutopurgeMessages WHERE GuildId = @GuildId AND ChannelId = @ChannelId AND MessageId IN {Sql.ToSqlObjectArray(messageIds)}",
                ("GuildId", row.GuildId),
                ("ChannelId", row.ChannelId));
        }
    }

    public class AutopurgeRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public TimeSpan Timespan { get; set; }
        public int Mode { get; set; }
        // 0 = All messages
        // 1 = Bot messages
        // 2 = Disabled
        // 3 = User messages

        private AutopurgeRow()
        {

        }

        public AutopurgeRow(ulong guildId, ulong channelId)
        {
            New = true;
            GuildId = guildId;
            ChannelId = channelId;
            Timespan = TimeSpan.FromMinutes(15);
            Mode = 2;
        }

        public static AutopurgeRow FromDatabase(ulong guildId, ulong channelId, string timespan, int mode)
        {
            return new()
            {
                New = false,
                GuildId = guildId,
                ChannelId = channelId,
                Timespan = TimeSpan.Parse(timespan),
                Mode = mode
            };
        }

        public async Task SaveAsync()
        {
            await Autopurge.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await Autopurge.DeleteRowAsync(this);
        }
    }

    public class AutopurgeMessageRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsBot { get; set; }
        public bool IsPinned { get; set; }

        public AutopurgeMessageRow()
        {
            New = true;
        }

        public static AutopurgeMessageRow FromDatabase(ulong guildId, ulong channelId, ulong messageId, DateTime timestamp, bool isBot, bool isPinned)
        {
            return new()
            {
                New = false,
                GuildId = guildId,
                ChannelId = channelId,
                MessageId = messageId,
                Timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc),
                IsBot = isBot,
                IsPinned = isPinned
            };
        }
    }
}
