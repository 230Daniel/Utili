using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class RoleCache
    {
        public static async Task<List<RoleCacheRow>> GetRowsAsync(ulong? guildId = null, ulong? userId = null)
        {
            List<RoleCacheRow> matchedRows = new();

            string command = "SELECT * FROM RoleCache WHERE TRUE";
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
                matchedRows.Add(RoleCacheRow.FromDatabase(
                    reader.GetUInt64(0),
                    reader.GetUInt64(1),
                    reader.GetString(2)));
            }

            reader.Close();

            return matchedRows;
        }

        public static async Task<RoleCacheRow> GetRowAsync(ulong guildId, ulong userId)
        {
            List<RoleCacheRow> rows = await GetRowsAsync(guildId, userId);
            return rows.Count > 0 ? rows.First() : new RoleCacheRow(guildId, userId);
        }

        public static async Task SaveRowAsync(RoleCacheRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO RoleCache (GuildId, UserId, RoleIds) VALUES (@GuildId, @UserId, @RoleIds);",
                    ("GuildId", row.GuildId),
                    ("UserId", row.UserId),
                    ("RoleIds", row.GetRoleIdsString()));

                row.New = false;
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE RoleCache SET GuildId = @GuildId, UserId = @UserId, RoleIds = @RoleIds WHERE GuildId = @GuildId AND UserId = @UserId;",
                    ("GuildId", row.GuildId), 
                    ("UserId", row.UserId),
                    ("RoleIds", row.GetRoleIdsString()));
            }
        }

        public static async Task DeleteRowAsync(RoleCacheRow row)
        {
            await Sql.ExecuteAsync(
                "DELETE FROM RoleCache WHERE GuildId = @GuildId AND UserId = @UserId;", 
                ("GuildId", row.GuildId), 
                ("UserId", row.UserId));
        }
    }

    public class RoleCacheRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public List<ulong> RoleIds { get; set; }

        private RoleCacheRow()
        {
        }

        public RoleCacheRow(ulong guildId, ulong userId)
        {
            New = true;
            GuildId = guildId;
            UserId = userId;
            RoleIds = new List<ulong>();
        }

        public static RoleCacheRow FromDatabase(ulong guildId, ulong userId, string roles)
        {
            RoleCacheRow row = new RoleCacheRow
            {
                New = false,
                GuildId = guildId,
                UserId = userId,
                RoleIds = new List<ulong>()
            };

            if (!string.IsNullOrWhiteSpace(roles))
                row.RoleIds = roles.Split(",").Select(ulong.Parse).ToList();

            return row;
        }

        public string GetRoleIdsString()
        {
            string rolesString = "";

            for (int i = 0; i < RoleIds.Count; i++)
            {
                ulong role = RoleIds[i];
                rolesString += role.ToString();
                if (i != RoleIds.Count - 1) rolesString += ",";
            }

            return rolesString;
        }

        public async Task SaveAsync()
        {
            await RoleCache.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await RoleCache.DeleteRowAsync(this);
        }
    }
}