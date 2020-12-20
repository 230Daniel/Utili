using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Core
    {
        public static List<CoreRow> GetRows(ulong? guildId = null, bool ignoreCache = false)
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
                List<(string, string)> values = new List<(string, string)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

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

        public static CoreRow GetRow(ulong guildId)
        {
            List<CoreRow> rows = GetRows(guildId);
            return rows.Count > 0 ? rows.First() : new CoreRow(guildId);
        }

        public static void SaveRow(CoreRow row)
        {
            MySqlCommand command;

            if (row.New)
            {
                command = Sql.GetCommand($"INSERT INTO Core (GuildId, Prefix, EnableCommands, ExcludedChannels) VALUES (@GuildId, @Prefix, {Sql.ToSqlBool(row.EnableCommands)}, @ExcludedChannels);",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("Prefix", row.Prefix.EncodedValue),
                        ("ExcludedChannels", row.GetExcludedChannelsString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
                row.New = false;
                
                if(Cache.Initialised) Cache.Core.Rows.Add(row);
            }
            else
            {
                command = Sql.GetCommand($"UPDATE Core SET Prefix = @Prefix, EnableCommands = {Sql.ToSqlBool(row.EnableCommands)}, ExcludedChannels = @ExcludedChannels WHERE GuildId = @GuildId;",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("Prefix", row.Prefix.EncodedValue),
                        ("ExcludedChannels", row.GetExcludedChannelsString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

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
            Prefix = EString.FromDecoded(Database._config.DefaultPrefix);
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
