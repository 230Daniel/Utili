using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class JoinRoles
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> joinRoles;
            if(oneGuildId == null) joinRoles = V1Data.GetDataList(type: "JoinRole");
            else joinRoles = V1Data.GetDataList(oneGuildId.ToString(), "JoinRole");

            foreach (V1Data v1JoinRole in joinRoles)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1JoinRole.GuildId);
                    JoinRolesRow row = new JoinRolesRow(guildId)
                    {
                        JoinRoles = new List<ulong> { ulong.Parse(v1JoinRole.Value) },
                    };
                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
