using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class RolePersist
    {
        public static async Task<List<RolePersistRow>> GetRowsAsync(ulong? guildId = null, bool ignoreCache = false)
        {
            List<RolePersistRow> matchedRows = new List<RolePersistRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.RolePersist);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM RolePersist WHERE TRUE";
                List<(string, object)> values = new List<(string, object)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(RolePersistRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetBoolean(1),
                        reader.GetString(2)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<RolePersistRow> GetRowAsync(ulong guildId)
        {
            List<RolePersistRow> rows = await GetRowsAsync(guildId);
            return rows.Count > 0 ? rows.First() : new RolePersistRow(guildId);
        }

        public static async Task SaveRowAsync(RolePersistRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO RolePersist (GuildId, Enabled, ExcludedRoles) VALUES (@GuildId, @Enabled, @ExcludedRoles);",
                    ("GuildId", row.GuildId),
                    ("Enabled", row.Enabled),
                    ("ExcludedRoles", row.GetExcludedRolesString()));

                row.New = false;
                if(Cache.Initialised) Cache.RolePersist.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE RolePersist SET Enabled = @Enabled, ExcludedRoles = @ExcludedRoles WHERE GuildId = @GuildId;",
                    ("GuildId", row.GuildId),
                    ("Enabled", row.Enabled),
                    ("ExcludedRoles", row.GetExcludedRolesString()));

                if(Cache.Initialised) Cache.RolePersist[Cache.RolePersist.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static async Task DeleteRowAsync(RolePersistRow row)
        {
            if(Cache.Initialised) Cache.RolePersist.RemoveAll(x => x.GuildId == row.GuildId);

            await Sql.ExecuteAsync(
                "DELETE FROM RolePersist WHERE GuildId = @GuildId", 
                ("GuildId", row.GuildId));
        }


        public static async Task<List<RolePersistRolesRow>> GetPersistRowsAsync(ulong? guildId = null, ulong? userId = null)
        {
            List<RolePersistRolesRow> matchedRows = new List<RolePersistRolesRow>();

            string command = "SELECT * FROM RolePersistRoles WHERE TRUE";
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
                matchedRows.Add(RolePersistRolesRow.FromDatabase(
                    reader.GetUInt64(0),
                    reader.GetUInt64(1),
                    reader.GetString(2)));
            }

            reader.Close();

            return matchedRows;
        }

        public static async Task<RolePersistRolesRow> GetPersistRowAsync(ulong guildId, ulong userId)
        {
            List<RolePersistRolesRow> rows = await GetPersistRowsAsync(guildId, userId);
            return rows.Count > 0 ? rows.First() : new RolePersistRolesRow(guildId, userId);
        }

        public static async Task SavePersistRowAsync(RolePersistRolesRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO RolePersistRoles (GuildId, UserId, Roles) VALUES (@GuildId, @UserId, @Roles);",
                    ("GuildId", row.GuildId),
                    ("UserId", row.UserId),
                    ("Roles", row.GetRolesString()));

                row.New = false;
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE RolePersistRoles SET GuildId = @GuildId, UserId = @UserId, Roles = @Roles WHERE GuildId = @GuildId AND UserId = @UserId;",
                    ("GuildId", row.GuildId), 
                    ("UserId", row.UserId),
                    ("Roles", row.GetRolesString()));
            }
        }

        public static async Task DeletePersistRowAsync(RolePersistRolesRow row)
        {
            await Sql.ExecuteAsync(
                "DELETE FROM RolePersistRoles WHERE GuildId = @GuildId AND UserId = @UserId;", 
                ("GuildId", row.GuildId), 
                ("UserId", row.UserId));
        }
    }

    public class RolePersistRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public bool Enabled { get; set; }
        public List<ulong> ExcludedRoles { get; set; }

        private RolePersistRow()
        {

        }

        public RolePersistRow(ulong guildId)
        {
            New = true;
            GuildId = guildId;
            Enabled = false;
            ExcludedRoles = new List<ulong>();
        }

        public static RolePersistRow FromDatabase(ulong guildId, bool enabled, string excludedRoles)
        {
            RolePersistRow row = new RolePersistRow
            {
                New = false,
                GuildId = guildId,
                Enabled = enabled,
                ExcludedRoles = new List<ulong>()
            };
            

            if (!string.IsNullOrEmpty(excludedRoles))
            {
                foreach (string excludedRole in excludedRoles.Split(","))
                {
                    if (ulong.TryParse(excludedRole, out ulong roleId))
                    {
                        row.ExcludedRoles.Add(roleId);
                    }
                }
            }

            return row;
        }

        public string GetExcludedRolesString()
        {
            string excludedRolesString = "";

            for (int i = 0; i < ExcludedRoles.Count; i++)
            {
                ulong excludedRole = ExcludedRoles[i];
                excludedRolesString += excludedRole.ToString();
                if (i != ExcludedRoles.Count - 1)
                {
                    excludedRolesString += ",";
                }
            }

            return excludedRolesString;
        }

        public async Task SaveAsync()
        {
            await RolePersist.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await RolePersist.DeleteRowAsync(this);
        }
    }

    public class RolePersistRolesRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public List<ulong> Roles { get; set; }

        private RolePersistRolesRow()
        {
        }

        public RolePersistRolesRow(ulong guildId, ulong userId)
        {
            New = true;
            GuildId = guildId;
            UserId = userId;
            Roles = new List<ulong>();
        }

        public static RolePersistRolesRow FromDatabase(ulong guildId, ulong userId, string roles)
        {
            RolePersistRolesRow row = new RolePersistRolesRow
            {
                New = false,
                GuildId = guildId,
                UserId = userId,
                Roles = new List<ulong>()
            };
            
            if (!string.IsNullOrEmpty(roles))
            {
                foreach (string role in roles.Split(","))
                {
                    if (ulong.TryParse(role, out ulong channelId))
                    {
                        row.Roles.Add(channelId);
                    }
                }
            }

            return row;
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

        public async Task SaveAsync()
        {
            await RolePersist.SavePersistRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await RolePersist.DeletePersistRowAsync(this);
        }
    }
}