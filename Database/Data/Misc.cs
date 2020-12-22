using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Misc
    {
        public static async Task<List<MiscRow>> GetRowsAsync(ulong? guildId = null, string type = null, string value = null, bool ignoreCache = false)
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
                List<(string, object)> values = new List<(string, object)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
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

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

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

        public static async Task<MiscRow> GetRowAsync(ulong? guildId = null, string type = null)
        {
            List<MiscRow> rows = await GetRowsAsync(guildId, type);
            return rows.Count == 0 ? null : rows.First();
        }

        public static async Task SaveRowAsync(MiscRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync("INSERT INTO Misc (GuildId, Type, Value) VALUES (@GuildId, @Type, @Value);",
                    ("GuildId", row.GuildId),
                    ("Type", row.Type),
                    ("Value", row.Value.EncodedValue));

                row.New = false;
                if(Cache.Initialised) Cache.Misc.Rows.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE Misc SET Value = @Value WHERE GuildId = @GuildId AND Type = @Type;",
                    ("GuildId", row.GuildId),
                    ("Type", row.Type),
                    ("Value", row.Value.EncodedValue));

                if(Cache.Initialised) Cache.Misc.Rows[Cache.Misc.Rows.FindIndex(x => x.GuildId == row.GuildId && x.Type == row.Type)] = row;
            }
        }

        public static async Task DeleteRowAsync(MiscRow row)
        {
            if(Cache.Initialised) Cache.Misc.Rows.RemoveAll(x => x.GuildId == row.GuildId && x.Type == row.Type);

            await Sql.ExecuteAsync(
                "DELETE FROM Misc WHERE GuildId = @GuildId AND Type = @Type",
                ("GuildId", row.GuildId),
                ("Type", row.Type));
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
