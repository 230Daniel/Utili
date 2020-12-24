using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Roles
    {
        public static async Task<List<RolesRow>> GetRowsAsync(ulong? guildId = null, bool ignoreCache = false)
        {
            List<RolesRow> matchedRows = new List<RolesRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Roles.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM Roles WHERE TRUE";
                List<(string, object)> values = new List<(string, object)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(RolesRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetBoolean(1),
                        reader.GetString(2)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<RolesRow> GetRowAsync(ulong guildId)
        {
            List<RolesRow> rows = await GetRowsAsync(guildId);
            return rows.Count > 0 ? rows.First() : new RolesRow(guildId);
        }

        public static async Task SaveRowAsync(RolesRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO Roles (GuildId, RolePersist, JoinRoles) VALUES (@GuildId, @RolePersist, @JoinRoles);",
                    ("GuildId", row.GuildId),
                    ("RolePersist", row.RolePersist),
                    ("JoinRoles", row.GetJoinRolesString()));

                row.New = false;
                if(Cache.Initialised) Cache.Roles.Rows.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE Roles SET RolePersist = @RolePersist, JoinRoles = @JoinRoles WHERE GuildId = @GuildId;",
                    ("GuildId", row.GuildId), 
                    ("RolePersist", row.RolePersist),
                    ("JoinRoles", row.GetJoinRolesString()));

                if(Cache.Initialised) Cache.Roles.Rows[Cache.Roles.Rows.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static async Task DeleteRowAsync(RolesRow row)
        {
            if(Cache.Initialised) Cache.Roles.Rows.RemoveAll(x => x.GuildId == row.GuildId);

            await Sql.ExecuteAsync(
                "DELETE FROM Roles WHERE GuildId = @GuildId", 
                ("GuildId", row.GuildId));
        }


        public static async Task<List<RolesPersistantRolesRow>> GetPersistRowsAsync(ulong? guildId = null, ulong? userId = null)
        {
            List<RolesPersistantRolesRow> matchedRows = new List<RolesPersistantRolesRow>();

            string command = "SELECT * FROM RolesPersistantRoles WHERE TRUE";
            List<(string, object)> values = new List<(string, object)>();

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
                matchedRows.Add(new RolesPersistantRolesRow(
                    reader.GetInt64(0),
                    reader.GetUInt64(1),
                    reader.GetUInt64(2),
                    reader.GetString(3)));
            }

            reader.Close();

            return matchedRows;
        }

        public static async Task<RolesPersistantRolesRow> GetPersistRowAsync(ulong guildId, ulong userId)
        {
            List<RolesPersistantRolesRow> rows = await GetPersistRowsAsync(guildId, userId);
            return rows.Count > 0 ? rows.First() : new RolesPersistantRolesRow(guildId, userId);
        }

        public static async Task SavePersistRowAsync(RolesPersistantRolesRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO RolesPersistantRoles (GuildId, UserId, Roles) VALUES (@GuildId, @UserId, @Roles);",
                    ("GuildId", row.GuildId),
                    ("UserId", row.UserId),
                    ("Roles", row.GetRolesString()));

                row.New = false;
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE RolesPersistantRoles SET GuildId = @GuildId, UserId = @UserId, Roles = @Roles WHERE GuildId = @GuildId AND UserId = @UserId;",
                    ("GuildId", row.GuildId), 
                    ("UserId", row.UserId),
                    ("Roles", row.GetRolesString()));
            }
        }

        public static async Task DeletePersistRowAsync(RolesPersistantRolesRow row)
        {
            await Sql.ExecuteAsync(
                "DELETE FROM RolesPersistantRoles WHERE GuildId = @GuildId AND UserId = @UserId;", 
                ("GuildId", row.GuildId), 
                ("UserId", row.UserId));
        }
    }

    public class RolesTable
    {
        public List<RolesRow> Rows { get; set; }
    }

    public class RolesRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public bool RolePersist { get; set; }
        public List<ulong> JoinRoles { get; set; }

        private RolesRow()
        {

        }

        public RolesRow(ulong guildId)
        {
            New = true;
            GuildId = guildId;
            RolePersist = false;
            JoinRoles = new List<ulong>();
        }

        public static RolesRow FromDatabase(ulong guildId, bool rolePersist, string joinRoles)
        {
            RolesRow row = new RolesRow
            {
                New = false,
                GuildId = guildId,
                RolePersist = rolePersist,
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
    }

    public class RolesPersistantRolesRow
    {
        // TODO: Refactor properly, these have to be unique as of current

        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public List<ulong> Roles { get; set; }

        private RolesPersistantRolesRow()
        {
        }

        public RolesPersistantRolesRow(ulong guildId, ulong userId)
        {
            New = true;
            GuildId = guildId;
            UserId = userId;
            Roles = new List<ulong>();
        }

        public RolesPersistantRolesRow(long id, ulong guildId, ulong userId, string roles)
        {
            New = false;
            GuildId = guildId;
            UserId = userId;

            Roles = new List<ulong>();

            if (!string.IsNullOrEmpty(roles))
            {
                foreach (string role in roles.Split(","))
                {
                    if (ulong.TryParse(role, out ulong channelId))
                    {
                        Roles.Add(channelId);
                    }
                }
            }
        }

        public string GetRolesString()
        {
            string rolesString = "";

            for (int i = 0; i < Roles.Count; i++)
            {
                ulong role = Roles[i];
                rolesString += role.ToString();
                if (i != Roles.Count - 1)
                {
                    rolesString += ",";
                }
            }

            return rolesString;
        }
    }
}