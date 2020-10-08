using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Database.Data
{
    public class Autopurge
    {
        public static List<AutopurgeRow> GetRowsWhere(ulong? guildId = null, ulong? channelId = null)
        {
            List<AutopurgeRow> matchedRows = new List<AutopurgeRow>();

            if (Cache.Initialised)
            {
                matchedRows = Cache.Autopurge.Rows;

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
                    matchedRows.Add(new AutopurgeRow(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetInt32(4),
                        reader.GetInt32(5)));
                }
            }

            return matchedRows;
        }

        public List<AutopurgeRow> GetRowsForGuilds(List<ulong> guilds)
        {
            return Cache.Autopurge.Rows.Where(x => guilds.Contains(x.GuildId)).ToList();
        }

        public static void SaveRow(AutopurgeRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO Autopurge (GuildID, ChannelId, Timespan, Mode, Messages) VALUES (@GuildId, @ChannelId, @Timespan, @Mode, @Messages);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Timespan", row.Timespan.ToString()),
                        ("Mode", row.Mode.ToString()),
                        ("Messages", row.Messages.ToString())});

                Cache.Autopurge.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE Autopurge WHERE Id = @Id SET (GuildID, ChannelId, Timespan, Mode, Messages) VALUES (@GuildId, @ChannelId, @Timespan, @Mode, @Messages);",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Timespan", row.Timespan.ToString()),
                        ("Mode", row.Mode.ToString()),
                        ("Messages", row.Messages.ToString())});

                Cache.Autopurge.Rows[Cache.Autopurge.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }

            command.ExecuteNonQuery();
        }

        public static void DeleteRow(AutopurgeRow row)
        {
            if(row == null) return;

            Cache.Autopurge.Rows.RemoveAll(x => x.Id == row.Id);

            string command = "DELETE FROM Autopurge WHERE Id = @Id";
            Sql.GetCommand(command, new[] {("Id", row.Id.ToString())}).ExecuteNonQuery();
        }
    }

    public class AutopurgeTable
    {
        public List<AutopurgeRow> Rows { get; set; }

        public void LoadAsync()
        // Load the table from the database
        {
            List<AutopurgeRow> newRows = new List<AutopurgeRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM Autopurge;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new AutopurgeRow(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetInt32(4),
                        reader.GetInt32(5)));
                }
            }
            catch {}

            Rows = newRows;
        }
    }

    public class AutopurgeRow
    {
        public int Id { get; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public TimeSpan Timespan { get; set; }
        public int Mode { get; set; }
        public int Messages { get; set; }

        public AutopurgeRow(int id, string guildId, string channelId, string timespan, int mode, int messages)
        {
            Id = id;
            GuildId = ulong.Parse(guildId);
            ChannelId = ulong.Parse(channelId);
            Timespan = TimeSpan.Parse(timespan);
            Mode = mode;
            Messages = messages;
        }
    }
}