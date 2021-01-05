using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class RolesPersistRoles
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> roles;
            if (oneGuildId == null) roles = V1Data.GetDataWhere("DataType LIKE '%RolePersist-Role-%'");
            else roles = V1Data.GetDataWhere($"GuildID = '{oneGuildId}' AND DataType LIKE '%RolePersist-Role-%'");

            List<RolesPersistantRolesRow> rows = new List<RolesPersistantRolesRow>();

            foreach (V1Data role in roles)
            {
                ulong guildId = ulong.Parse(role.GuildId);
                ulong userId = ulong.Parse(role.Type.Split("-").Last());
                ulong roleId = ulong.Parse(role.Value);

                if (rows.Any(x => x.GuildId == guildId && x.UserId == userId))
                {
                    RolesPersistantRolesRow row = rows.First(x => x.GuildId == guildId && x.UserId == userId);
                    if (!row.Roles.Contains(roleId)) row.Roles.Add(roleId);
                }
                else
                {
                    RolesPersistantRolesRow row = new RolesPersistantRolesRow(guildId, userId);
                    row.Roles.Add(roleId);
                    rows.Add(row);
                }
            }

            Program.RowsToSave.AddRange(rows);
        }
    }
}
