using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal class InactiveRoleUsers
    {
        public static async Task TransferAsync(ulong? guildId = null)
        {
            List<V1Data> v1Datas = V1Data.GetDataList(guildId.ToString(), ignoreCache: true, table: "Utili_InactiveTimers");

            foreach (V1Data v1 in v1Datas)
            {

                try
                {
                    InactiveRoleUserRow row = new InactiveRoleUserRow(ulong.Parse(v1.GuildId), ulong.Parse(v1.Type.Split("-").Last()), DateTime.Parse(v1.Value));
                    await InactiveRole.UpdateUserAsync(row.GuildId, row.UserId, row.LastAction);
                    Console.WriteLine($"Done {row.GuildId}-{row.UserId}");
                }
                catch { }
            }
        }
    }
}
