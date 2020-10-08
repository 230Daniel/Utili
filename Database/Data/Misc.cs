using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public class Misc
    {
        public static List<MiscRow> GetRowsWhere(ulong? guildId = null, string type = null)
        {
            List<MiscRow> matchedRows = new List<MiscRow>();

            if (Cache.Initialised)
            {
                matchedRows = Cache.Misc.Rows;

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (type != null) matchedRows.RemoveAll(x => x.Type != type);
            }
            else
            {
                string command = "SELECT * FROM Misc WHERE TRUE";
                List<(string, string)> values = new List<(string, string)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                if (type != null)
                {
                    command += " AND Type = @Type";
                    values.Add(("Type", type));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(new MiscRow(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3)));
                }
            }

            return matchedRows;
        }

        public static void SaveRow(MiscRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO Misc (GuildID, Type, Value) VALUES (@GuildId, @Type, @Value);",
                    new[]
                    {
                        ("GuildId", row.GuildId.ToString()),
                        ("Type", row.Type),
                        ("Value", row.Value)});
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE Misc WHERE Id = @Id SET (GuildID, Type, Value) VALUES (@GuildId, @Type, @Value);",
                    new[]
                    {
                        ("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()),
                        ("Type", row.Type),
                        ("Value", row.Value)});
            }
        }
    }

    public class MiscTable
    {
        public List<MiscRow> Rows { get; set; }

        public void LoadAsync()
        // Load the table from the database
        {
            List<MiscRow> newRows = new List<MiscRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM Misc;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new MiscRow(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3)));
                }
            }
            catch {}

            Rows = newRows;
        }
    }

    public class MiscRow
    {
        public int Id { get; }
        public ulong GuildId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public MiscRow(int id, string guildId, string type, string value)
        {
            Id = id;
            GuildId = ulong.Parse(guildId);
            Type = type;
            Value = value;
        }
    }
}
