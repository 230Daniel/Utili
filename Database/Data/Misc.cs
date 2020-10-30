using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public class Misc
    {
        public static List<MiscRow> GetRows(ulong? guildId = null, string type = null, string value = null, bool ignoreCache = false)
        {
            List<MiscRow> matchedRows = new List<MiscRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Misc.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (type != null) matchedRows.RemoveAll(x => x.Type != type);
                if (value != null) matchedRows.RemoveAll(x => x.Value != value);
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

                if (value != null)
                {
                    command += " AND Value = @Value";
                    values.Add(("Value", value));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(new MiscRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetString(2),
                        reader.GetString(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static MiscRow GetRow(ulong? guildId = null, string type = null)
        {
            List<MiscRow> rows = GetRows(guildId, type);
            if (rows.Count == 0) return null; 
            return rows.First();
        }

        public static void SaveRow(MiscRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO Misc (GuildId, Type, Value) VALUES (@GuildId, @Type, @Value);",
                    new[]
                    {
                        ("GuildId", row.GuildId.ToString()),
                        ("Type", row.Type),
                        ("Value", row.Value)});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.Id = GetRows(row.GuildId, row.Type, row.Value).First().Id;

                if(Cache.Initialised) Cache.Misc.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE Misc SET GuildId = @GuildId, Type = @Type, Value = @Value WHERE Id = @Id;",
                    new[]
                    {
                        ("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()),
                        ("Type", row.Type),
                        ("Value", row.Value)});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.Misc.Rows[Cache.Misc.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void DeleteRow(MiscRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.Misc.Rows.RemoveAll(x => x.Id == row.Id);

            string commandText = "DELETE FROM Misc WHERE Id = @Id";
            MySqlCommand command = Sql.GetCommand(commandText, new[] {("Id", row.Id.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }

        public static string GetPrefix(ulong guildId)
        {
            string prefix = ".";

            var rows = GetRows(guildId, "Prefix");
            if (rows.Count > 0) prefix = rows.First().Value;

            return prefix;
        }

        public static void SetPrefix(ulong guildId, string prefix)
        {
            MiscRow row = GetRow(guildId, "Prefix");

            if (prefix == ".")
            {
                DeleteRow(row);
                return;
            }

            if (row == null)
            {
                row = new MiscRow(guildId, "Prefix", prefix);
            }

            row.Value = prefix;

            SaveRow(row);
        }
    }

    public class MiscTable
    {
        public List<MiscRow> Rows { get; set; }

        public void Load()
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
                        reader.GetUInt64(1),
                        reader.GetString(2),
                        reader.GetString(3)));
                }
            }
            catch {}

            reader.Close();

            Rows = newRows;
        }
    }

    public class MiscRow
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        public MiscRow(ulong guildId, string type, string value)
        {
            Id = 0;
            GuildId = guildId;
            Type = type;
            Value = value;
        }

        public MiscRow(int id, ulong guildId, string type, string value)
        {
            Id = id;
            GuildId = guildId;
            Type = type;
            Value = value;
        }
    }
}
