using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Autopurge
    {
        public static async Task<List<AutopurgeRow>> GetRowsAsync(ulong? guildId = null, ulong? channelId = null, bool enabledOnly = false, bool ignoreCache = false)
        {
            List<AutopurgeRow> matchedRows = new List<AutopurgeRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Autopurge.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
            }
            else
            {
                string command = "SELECT * FROM Autopurge WHERE TRUE";
                List<(string, object)> values = new List<(string, object)>();

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

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

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
            List<AutopurgeRow> rows = await GetRowsAsync(guildId, channelId);
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

                if(Cache.Initialised) Cache.Autopurge.Rows.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE Autopurge SET Timespan = @Timespan, Mode = @Mode WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("Timespan", row.Timespan),
                    ("Mode", row.Mode));

                if(Cache.Initialised) Cache.Autopurge.Rows[Cache.Autopurge.Rows.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static async Task DeleteRowAsync(AutopurgeRow row)
        {
            if(Cache.Initialised) Cache.Autopurge.Rows.RemoveAll(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId);

            await Sql.ExecuteAsync(
                "DELETE FROM Autopurge WHERE GuildId = @GuildId AND ChannelId = @ChannelId",
                ("GuildId", row.GuildId),
                ("ChannelId", row.ChannelId));
        }
    }

    public class AutopurgeTable
    {
        public List<AutopurgeRow> Rows { get; set; }
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
            return new AutopurgeRow
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
}
