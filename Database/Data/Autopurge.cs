using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Autopurge
    {
        public static List<AutopurgeRow> GetRows(ulong? guildId = null, ulong? channelId = null, bool ignoreCache = false)
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

        public static void SaveRow(AutopurgeRow row)
        {
            MySqlCommand command;

            if (row.New)
            {
                command = Sql.GetCommand("INSERT INTO Autopurge (GuildId, ChannelId, Timespan, Mode) VALUES (@GuildId, @ChannelId, @Timespan, @Mode);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Timespan", row.Timespan.ToString()),
                        ("Mode", row.Mode.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;

                if(Cache.Initialised) Cache.Autopurge.Rows.Add(row);
            }
            else
            {
                command = Sql.GetCommand("UPDATE Autopurge SET Timespan = @Timespan, Mode = @Mode WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Timespan", row.Timespan.ToString()),
                        ("Mode", row.Mode.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.Autopurge.Rows[Cache.Autopurge.Rows.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static void DeleteRow(AutopurgeRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.Autopurge.Rows.RemoveAll(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId);

            string commandText = "DELETE FROM Autopurge WHERE GuildId = @GuildId AND ChannelId = @ChannelId";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {
                ("GuildId", row.GuildId.ToString()),
                ("ChannelId", row.ChannelId.ToString())});

            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class AutopurgeTable
    {
        public List<AutopurgeRow> Rows { get; set; }
    }

    public class AutopurgeRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public TimeSpan Timespan { get; set; }
        public int Mode { get; set; }
        // 0 = All messages
        // 1 = Bot messages

        public AutopurgeRow()
        {
            New = true;
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
    }
}
