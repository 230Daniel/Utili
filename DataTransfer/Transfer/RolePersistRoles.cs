using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class RolePersistRoles
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> roles;
            if (oneGuildId == null) roles = V1Data.GetDataWhere("DataType LIKE '%RolePersist-Role-%'");
            else roles = V1Data.GetDataWhere($"GuildID = '{oneGuildId}' AND DataType LIKE '%RolePersist-Role-%'");

            List<RolePersistRolesRow> rows = new List<RolePersistRolesRow>();

            foreach (V1Data role in roles)
            {
                ulong guildId = ulong.Parse(role.GuildId);
                ulong userId = ulong.Parse(role.Type.Split("-").Last());
                ulong roleId = ulong.Parse(role.Value);

                if (rows.Any(x => x.GuildId == guildId && x.UserId == userId))
                {
                    RolePersistRolesRow row = rows.First(x => x.GuildId == guildId && x.UserId == userId);
                    if (!row.Roles.Contains(roleId)) row.Roles.Add(roleId);
                }
                else
                {
                    RolePersistRolesRow row = new RolePersistRolesRow(guildId, userId);
                    row.Roles.Add(roleId);
                    rows.Add(row);
                }
            }

            Program.RowsToSave.AddRange(rows);
        }
    }
}
