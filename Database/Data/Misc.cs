using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Data
{
    public static class Misc
    {
        public static async Task<List<MiscRow>> GetRowsAsync(ulong? guildId = null, string type = null, string value = null, bool ignoreCache = false)
        {
            List<MiscRow> matchedRows = new();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Misc);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (type is not null) matchedRows.RemoveAll(x => x.Type != type);
                if (value is not null) matchedRows.RemoveAll(x => x.Value != value);
            }
            else
            {
                var command = "SELECT * FROM Misc WHERE TRUE";
                List<(string, object)> values = new();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                if (type is not null)
                {
                    command += " AND Type = @Type";
                    values.Add(("Type", type));
                }

                if (value is not null)
                {
                    command += " AND Value = @Value";
                    values.Add(("Value", EString.FromDecoded(value).EncodedValue));
                }

                var reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

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
            var rows = await GetRowsAsync(guildId, type);
            return rows.Count == 0 ? null : rows.First();
        }

        public static async Task SaveRowAsync(MiscRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync("INSERT INTO Misc (GuildId, Type, Value) VALUES (@GuildId, @Type, @Value);",
                    ("GuildId", row.GuildId),
                    ("Type", row.Type),
                    ("Value", row.Value));

                row.New = false;
                if(Cache.Initialised) Cache.Misc.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE Misc SET Type = @Type, Value = @Value WHERE GuildId = @GuildId AND Type = @Type AND Value = @Value;",
                    ("GuildId", row.GuildId),
                    ("Type", row.Type),
                    ("Value", row.Value));

                if(Cache.Initialised) Cache.Misc[Cache.Misc.FindIndex(x => x.GuildId == row.GuildId && x.Type == row.Type)] = row;
            }
        }

        public static async Task DeleteRowAsync(MiscRow row)
        {
            if(Cache.Initialised) Cache.Misc.RemoveAll(x => x.GuildId == row.GuildId && x.Type == row.Type && x.Value == row.Value);

            await Sql.ExecuteAsync(
                "DELETE FROM Misc WHERE GuildId = @GuildId AND Type = @Type AND Value = @Value",
                ("GuildId", row.GuildId),
                ("Type", row.Type),
                ("Value", row.Value));
        }
    }

    public class MiscRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }

        private MiscRow()
        {

        }

        public MiscRow(ulong guildId, string type, string value)
        {
            New = true;
            GuildId = guildId;
            Type = type;
            Value = value;
        }

        public static MiscRow FromDatabase(ulong guildId, string type, string value)
        {
            return new()
            {
                New = false,
                GuildId = guildId,
                Type = type,
                Value = value
            };
        }

        public async Task SaveAsync()
        {
            await Misc.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await Misc.DeleteRowAsync(this);
        }
    }
}
