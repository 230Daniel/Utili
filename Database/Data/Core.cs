using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Core
    {
        public static async Task<List<CoreRow>> GetRowsAsync(ulong? guildId = null, bool ignoreCache = false)
        {
            List<CoreRow> matchedRows = new List<CoreRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Core.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM Core WHERE TRUE";
                List<(string, object)> values = new List<(string, object)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

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
            List<CoreRow> rows = await GetRowsAsync(guildId);
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
                if(Cache.Initialised) Cache.Core.Rows.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE Core SET Prefix = @Prefix, EnableCommands = @EnableCommands, ExcludedChannels = @ExcludedChannels WHERE GuildId = @GuildId;",
                    ("GuildId", row.GuildId),
                    ("Prefix", row.Prefix.EncodedValue),
                    ("EnableCommands", row.EnableCommands),
                    ("ExcludedChannels", row.GetExcludedChannelsString()));

                if(Cache.Initialised) Cache.Core.Rows[Cache.Core.Rows.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }
    }

    public class CoreTable
    {
        public List<CoreRow> Rows { get; set; }
    }

    public class CoreRow
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
            CoreRow row = new CoreRow
            {
                New = false,
                GuildId = guildId,
                Prefix = EString.FromEncoded(prefix),
                EnableCommands = enableCommands,
                ExcludedChannels = new List<ulong>()
            };

            if (!string.IsNullOrEmpty(excludedChannels))
            {
                foreach (string excludedChannel in excludedChannels.Split(","))
                {
                    if (ulong.TryParse(excludedChannel, out ulong channelId))
                    {
                        row.ExcludedChannels.Add(channelId);
                    }
                }
            }

            return row;
        }

        public string GetExcludedChannelsString()
        {
            string excludedChannelsString = "";

            for (int i = 0; i < ExcludedChannels.Count; i++)
            {
                ulong excludedChannelId = ExcludedChannels[i];
                excludedChannelsString += excludedChannelId.ToString();
                if (i != ExcludedChannels.Count - 1)
                {
                    excludedChannelsString += ",";
                }
            }

            return excludedChannelsString;
        }
    }
}
