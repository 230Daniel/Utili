using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class Roles
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> joinRoles;
            if(oneGuildId == null) joinRoles = V1Data.GetDataList(type: "JoinRole");
            else joinRoles = V1Data.GetDataList(oneGuildId.ToString(), "JoinRole");

            List<V1Data> rolePersists;
            if(oneGuildId == null) rolePersists = V1Data.GetDataList(type: "RolePersist-Enabled");
            else rolePersists = V1Data.GetDataList(oneGuildId.ToString(), "RolePersist-Enabled");

            foreach (V1Data v1JoinRole in joinRoles)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1JoinRole.GuildId);
                    RolesRow row = new RolesRow(guildId)
                    {
                        JoinRoles = new List<ulong> { ulong.Parse(v1JoinRole.Value) },
                        RolePersist = rolePersists.Any(x => x.GuildId == v1JoinRole.GuildId)
                    };
                    Program.RowsToSave.Add(row);
                }
                catch { }
            }

            foreach (ulong guildId in rolePersists.Select(x => ulong.Parse(x.GuildId)).Distinct().Where(x => joinRoles.All(y => y.GuildId != x.ToString())))
            {
                try
                {
                    RolesRow row = new RolesRow(guildId)
                    {
                        JoinRoles = new List<ulong>(),
                        RolePersist = true
                    };
                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
