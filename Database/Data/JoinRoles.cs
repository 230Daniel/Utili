using System;
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
        
        public static async Task<List<JoinRolesPendingRow>> GetPendingRowsAsync(ulong? guildId = null, ulong? userId = null)
        {
            List<JoinRolesPendingRow> matchedRows = new();

            string command = "SELECT * FROM JoinRolesPending WHERE TRUE";
            List<(string, object)> values = new();

            if (guildId.HasValue)
            {
                command += " AND GuildId = @GuildId";
                values.Add(("GuildId", guildId.Value));
            }

            if (userId.HasValue)
            {
                command += " AND UserId = @UserId";
                values.Add(("UserId", userId.Value));
            }

            MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

            while (reader.Read())
            {
                matchedRows.Add(JoinRolesPendingRow.FromDatabase(
                    reader.GetUInt64(0),
                    reader.GetUInt64(1),
                    reader.GetBoolean(2),
                    reader.GetDateTime(3)));
            }

            reader.Close();

            return matchedRows;
        }

        public static async Task<JoinRolesPendingRow> GetPendingRowAsync(ulong guildId, ulong userId)
        {
            List<JoinRolesPendingRow> rows = await GetPendingRowsAsync(guildId, userId);
            return rows.Count > 0 ? rows.First() : new JoinRolesPendingRow(guildId, userId);
        }

        public static async Task SavePendingRowAsync(JoinRolesPendingRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO JoinRolesPending (GuildId, UserId, IsPending, ScheduledFor) VALUES (@GuildId, @UserId, @IsPending, @ScheduledFor);",
                    ("GuildId", row.GuildId),
                    ("UserId", row.UserId),
                    ("IsPending", row.IsPending),
                    ("ScheduledFor", row.ScheduledFor));

                row.New = false;
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE JoinRolesPending SET IsPending = @IsPending, ScheduledFor = @ScheduledFor WHERE GuildId = @GuildId AND UserId = @UserId;",
                    ("GuildId", row.GuildId),
                    ("UserId", row.UserId),
                    ("IsPending", row.IsPending),
                    ("ScheduledFor", row.ScheduledFor));
            }
        }

        public static async Task DeletePendingRowAsync(JoinRolesPendingRow row)
        {
            await Sql.ExecuteAsync(
                "DELETE FROM JoinRolesPending WHERE GuildId = @GuildId AND UserId = @UserId;", 
                ("GuildId", row.GuildId),
                ("UserId", row.UserId));
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

    public class JoinRolesPendingRow : IRow
    {
        public bool New { get; set; }

        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public bool IsPending { get; set; }
        public DateTime ScheduledFor { get; set; }

        JoinRolesPendingRow() { }

        public JoinRolesPendingRow(ulong guildId, ulong userId)
        {
            New = true;
            GuildId = guildId;
            UserId = userId;
            ScheduledFor = DateTime.MinValue;
        }

        public static JoinRolesPendingRow FromDatabase(ulong guildId, ulong userId, bool isPending, DateTime scheduledFor)
        {
            return new()
            {
                New = false,
                GuildId = guildId,
                UserId = userId,
                IsPending = isPending,
                ScheduledFor = scheduledFor
            };
        }
        
        public Task SaveAsync()
        {
            return JoinRoles.SavePendingRowAsync(this);
        }

        public Task DeleteAsync()
        {
            return JoinRoles.DeletePendingRowAsync(this);
        }
    }
}