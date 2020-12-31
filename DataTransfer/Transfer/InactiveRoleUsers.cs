using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class InactiveRoleUsers
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> v1Rows;
            if(oneGuildId == null) v1Rows = V1Data.GetDataList(ignoreCache: true, table: "Utili_InactiveTimers");
            else v1Rows = V1Data.GetDataList(oneGuildId.ToString(), ignoreCache: true, table: "Utili_InactiveTimers");

            foreach (V1Data v1 in v1Rows)
            {
                try
                {
                    InactiveRoleUserRow row = new InactiveRoleUserRow(ulong.Parse(v1.GuildId), ulong.Parse(v1.Type.Split("-").Last()), DateTime.Parse(v1.Value));
                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
