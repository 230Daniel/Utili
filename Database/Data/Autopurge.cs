using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Autopurge
    {
        public static List<AutopurgeRow> GetRows(ulong? guildId = null, ulong? channelId = null, long? id = null, bool ignoreCache = false)
        {
            List<AutopurgeRow> matchedRows = new List<AutopurgeRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Autopurge.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
                if (id.HasValue) matchedRows.RemoveAll(x => x.Id != id.Value);
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

                if (id.HasValue)
                {
                    command += " AND Id = @Id";
                    values.Add(("Id", id.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(new AutopurgeRow(
                        reader.GetInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetString(3),
                        reader.GetInt32(4)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static void SaveRow(AutopurgeRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO Autopurge (GuildId, ChannelId, Timespan, Mode) VALUES (@GuildId, @ChannelId, @Timespan, @Mode);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Timespan", row.Timespan.ToString()),
                        ("Mode", row.Mode.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.Id = GetRows(row.GuildId, row.ChannelId, ignoreCache: true).First().Id;

                if(Cache.Initialised) Cache.Autopurge.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE Autopurge SET GuildId = @GuildId, ChannelId = @ChannelId, Timespan = @Timespan, Mode = @Mode WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Timespan", row.Timespan.ToString()),
                        ("Mode", row.Mode.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.Autopurge.Rows[Cache.Autopurge.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void DeleteRow(AutopurgeRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.Autopurge.Rows.RemoveAll(x => x.Id == row.Id);

            string commandText = "DELETE FROM Autopurge WHERE Id = @Id";
            MySqlCommand command = Sql.GetCommand(commandText, new[] {("Id", row.Id.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class AutopurgeTable
    {
        public List<AutopurgeRow> Rows { get; set; }

        public void Load()
        // Load the table from the database
        {
            List<AutopurgeRow> newRows = new List<AutopurgeRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM Autopurge;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new AutopurgeRow(
                        reader.GetInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetString(3),
                        reader.GetInt32(4)));
                }
            }
            catch {}

            reader.Close();

            Rows = newRows;
        }
    }

    public class AutopurgeRow
    {
        public long Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public TimeSpan Timespan { get; set; }
        public int Mode { get; set; }
        // 0 = All messages
        // 1 = Bot messages

        public AutopurgeRow()
        {
            Id = 0;
        }

        public AutopurgeRow(long id, ulong guildId, ulong channelId, string timespan, int mode)
        {
            Id = id;
            GuildId = guildId;
            ChannelId = channelId;
            Timespan = TimeSpan.Parse(timespan);
            Mode = mode;
        }
    }
}
