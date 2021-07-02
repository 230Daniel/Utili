using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Data
{
    public static class Core
    {
        public static async Task<List<CoreRow>> GetRowsAsync(ulong? guildId = null, bool ignoreCache = false)
        {
            List<CoreRow> matchedRows = new();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Core);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                var command = "SELECT * FROM Core WHERE TRUE";
                List<(string, object)> values = new();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                var reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(CoreRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetString(1),
                        reader.GetBoolean(2),
                        reader.GetString(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<CoreRow> GetRowAsync(ulong guildId)
        {
            var rows = await GetRowsAsync(guildId);
            return rows.Count > 0 ? rows.First() : new CoreRow(guildId);
        }

        public static async Task SaveRowAsync(CoreRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO Core (GuildId, Prefix, EnableCommands, ExcludedChannels) VALUES (@GuildId, @Prefix, @EnableCommands, @ExcludedChannels);",
                    ("GuildId", row.GuildId), 
                    ("Prefix", row.Prefix.EncodedValue),
                    ("EnableCommands", row.EnableCommands),
                    ("ExcludedChannels", row.GetExcludedChannelsString()));

                row.New = false;
                if(Cache.Initialised) Cache.Core.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE Core SET Prefix = @Prefix, EnableCommands = @EnableCommands, ExcludedChannels = @ExcludedChannels WHERE GuildId = @GuildId;",
                    ("GuildId", row.GuildId),
                    ("Prefix", row.Prefix.EncodedValue),
                    ("EnableCommands", row.EnableCommands),
                    ("ExcludedChannels", row.GetExcludedChannelsString()));

                if(Cache.Initialised) Cache.Core[Cache.Core.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static async Task DeleteRowAsync(CoreRow row)
        {
            if(Cache.Initialised) Cache.Core.RemoveAll(x => x.GuildId == row.GuildId);

            await Sql.ExecuteAsync(
                "DELETE FROM Core WHERE GuildId = @GuildId",
                ("GuildId", row.GuildId));
        }
    }
    public class CoreRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public EString Prefix { get; set; }
        public bool EnableCommands { get; set; }
        public List<ulong> ExcludedChannels { get; set; }

        private CoreRow()
        {

        }

        public CoreRow(ulong guildId)
        {
            New = true;
            GuildId = guildId;
            Prefix = EString.FromDecoded(Database.Config.DefaultPrefix);
            EnableCommands = true;
            ExcludedChannels = new List<ulong>();
        }

        public static CoreRow FromDatabase(ulong guildId, string prefix, bool enableCommands, string excludedChannels)
        {
            CoreRow row = new()
            {
                New = false,
                GuildId = guildId,
                Prefix = EString.FromEncoded(prefix),
                EnableCommands = enableCommands,
                ExcludedChannels = new List<ulong>()
            };

            if (!string.IsNullOrEmpty(excludedChannels))
            {
                foreach (var excludedChannel in excludedChannels.Split(","))
                {
                    if (ulong.TryParse(excludedChannel, out var channelId))
                    {
                        row.ExcludedChannels.Add(channelId);
                    }
                }
            }

            return row;
        }

        public string GetExcludedChannelsString()
        {
            var excludedChannelsString = "";

            for (var i = 0; i < ExcludedChannels.Count; i++)
            {
                var excludedChannelId = ExcludedChannels[i];
                excludedChannelsString += excludedChannelId.ToString();
                if (i != ExcludedChannels.Count - 1)
                {
                    excludedChannelsString += ",";
                }
            }

            return excludedChannelsString;
        }

        public async Task SaveAsync()
        {
            await Core.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await Core.DeleteRowAsync(this);
        }
    }
}
