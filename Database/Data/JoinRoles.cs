using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class JoinRoles
    {
        public static async Task<List<JoinRolesRow>> GetRowsAsync(ulong? guildId = null, bool ignoreCache = false)
        {
            List<JoinRolesRow> matchedRows = new();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.JoinRoles);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM JoinRoles WHERE TRUE";
                List<(string, object)> values = new();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(JoinRolesRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetBoolean(1),
                        reader.GetString(2)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<JoinRolesRow> GetRowAsync(ulong guildId)
        {
            List<JoinRolesRow> rows = await GetRowsAsync(guildId);
            return rows.Count > 0 ? rows.First() : new JoinRolesRow(guildId);
        }

        public static async Task SaveRowAsync(JoinRolesRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO JoinRoles (GuildId, WaitForVerification, JoinRoles) VALUES (@GuildId, @WaitForVerification, @JoinRoles);",
                    ("GuildId", row.GuildId),
                    ("WaitForVerification", row.WaitForVerification),
                    ("JoinRoles", row.GetJoinRolesString()));

                row.New = false;
                if(Cache.Initialised) Cache.JoinRoles.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE JoinRoles SET WaitForVerification = @WaitForVerification, JoinRoles = @JoinRoles WHERE GuildId = @GuildId;",
                    ("GuildId", row.GuildId),
                    ("WaitForVerification", row.WaitForVerification),
                    ("JoinRoles", row.GetJoinRolesString()));

                if(Cache.Initialised) Cache.JoinRoles[Cache.JoinRoles.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static async Task DeleteRowAsync(JoinRolesRow row)
        {
            if(Cache.Initialised) Cache.JoinRoles.RemoveAll(x => x.GuildId == row.GuildId);

            await Sql.ExecuteAsync(
                "DELETE FROM JoinRoles WHERE GuildId = @GuildId", 
                ("GuildId", row.GuildId));
        }
    }

    public class JoinRolesRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public bool WaitForVerification { get; set; }
        public List<ulong> JoinRoles { get; set; }

        private JoinRolesRow()
        {

        }

        public JoinRolesRow(ulong guildId)
        {
            New = true;
            GuildId = guildId;
            WaitForVerification = false;
            JoinRoles = new List<ulong>();
        }

        public static JoinRolesRow FromDatabase(ulong guildId, bool waitForVerification, string joinRoles)
        {
            JoinRolesRow row = new()
            {
                New = false,
                WaitForVerification = waitForVerification,
                GuildId = guildId,
                JoinRoles = new List<ulong>()
            };
            

            if (!string.IsNullOrEmpty(joinRoles))
            {
                foreach (string joinRole in joinRoles.Split(","))
                {
                    if (ulong.TryParse(joinRole, out ulong channelId))
                    {
                        row.JoinRoles.Add(channelId);
                    }
                }
            }

            return row;
        }

        public string GetJoinRolesString()
        {
            string joinRolesString = "";

            for (int i = 0; i < JoinRoles.Count; i++)
            {
                ulong joinRole = JoinRoles[i];
                joinRolesString += joinRole.ToString();
                if (i != JoinRoles.Count - 1)
                {
                    joinRolesString += ",";
                }
            }

            return joinRolesString;
        }

        public async Task SaveAsync()
        {
            await Data.JoinRoles.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await Data.JoinRoles.DeleteRowAsync(this);
        }
    }
}