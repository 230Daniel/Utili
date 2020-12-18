using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Misc
    {
        public static List<MiscRow> GetRows(ulong? guildId = null, string type = null, string value = null, bool ignoreCache = false)
        {
            List<MiscRow> matchedRows = new List<MiscRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Misc.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (type != null) matchedRows.RemoveAll(x => x.Type != type);
                if (value != null) matchedRows.RemoveAll(x => x.Value.Value != value);
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
                    values.Add(("Value", EString.FromDecoded(value).EncodedValue));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(MiscRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetString(1),
                        reader.GetString(2)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static MiscRow GetRow(ulong? guildId = null, string type = null)
        {
            List<MiscRow> rows = GetRows(guildId, type);
            return rows.Count == 0 ? null : rows.First();
        }

        public static void SaveRow(MiscRow row)
        {
            MySqlCommand command;

            if (row.New)
            {
                command = Sql.GetCommand("INSERT INTO Misc (GuildId, Type, Value) VALUES (@GuildId, @Type, @Value);",
                    new[]
                    {
                        ("GuildId", row.GuildId.ToString()),
                        ("Type", row.Type),
                        ("Value", row.Value.EncodedValue)});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;

                if(Cache.Initialised) Cache.Misc.Rows.Add(row);
            }
            else
            {
                command = Sql.GetCommand("UPDATE Misc SET Value = @Value WHERE GuildId = @GuildId AND Type = @Type;",
                    new[]
                    {
                        ("GuildId", row.GuildId.ToString()),
                        ("Type", row.Type),
                        ("Value", row.Value.EncodedValue)
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.Misc.Rows[Cache.Misc.Rows.FindIndex(x => x.GuildId == row.GuildId && x.Type == row.Type)] = row;
            }
        }

        public static void DeleteRow(MiscRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.Misc.Rows.RemoveAll(x => x.GuildId == row.GuildId && x.Type == row.Type);

            string commandText = "DELETE FROM Misc WHERE GuildId = @GuildId AND Type = @Type";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {
                    ("GuildId", row.GuildId.ToString()),
                    ("Type", row.Type)});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }

        public static string GetPrefix(ulong guildId)
        {
            string prefix = "b.";

            List<MiscRow> rows = GetRows(guildId, "Prefix");
            if (rows.Count > 0) prefix = rows.First().Value.Value;

            return prefix;
        }

        public static void SetPrefix(ulong guildId, string prefix)
        {
            MiscRow row = GetRow(guildId, "Prefix");

            if(prefix == "b." && row == null) return;

            if (prefix == "b.")
            {
                DeleteRow(row);
                return;
            }

            if (row == null)
            {
                row = new MiscRow(guildId, "Prefix", prefix);
            }

            row.Value = EString.FromDecoded(prefix);
            SaveRow(row);
        }
    }

    public class MiscTable
    {
        public List<MiscRow> Rows { get; set; }
    }

    public class MiscRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public string Type { get; set; }
        public EString Value { get; set; }

        private MiscRow()
        {

        }

        public MiscRow(ulong guildId, string type, string value)
        {
            New = true;
            GuildId = guildId;
            Type = type;
            Value = EString.FromDecoded(value);
        }

        public static MiscRow FromDatabase(ulong guildId, string type, string value)
        {
            return new MiscRow
            {
                New = false,
                GuildId = guildId,
                Type = type,
                Value = EString.FromEncoded(value)
            };
        }
    }
}
