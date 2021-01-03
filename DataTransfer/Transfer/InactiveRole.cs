using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class InactiveRole
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> roles;
            if(oneGuildId == null) roles = V1Data.GetDataList(type: "InactiveRole-Role");
            else roles = V1Data.GetDataList(oneGuildId.ToString(), "InactiveRole-Role");

            foreach (V1Data v1Role in roles)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1Role.GuildId);
                    ulong roleId = ulong.Parse(v1Role.Value);

                    ulong immuneRoleId = 0;
                    try { immuneRoleId = ulong.Parse(V1Data.GetFirstData(guildId.ToString(), "InactiveRole-ImmuneRole").Value); } catch { }

                    string mode = "Give";
                    try { mode = V1Data.GetFirstData(guildId.ToString(), "InactiveRole-Mode").Value; } catch { }

                    TimeSpan timespan = TimeSpan.FromDays(30);
                    try { timespan = TimeSpan.Parse(V1Data.GetFirstData(guildId.ToString(), "InactiveRole-Timespan").Value); } catch { }

                    InactiveRoleRow row = InactiveRoleRow.FromDatabase(guildId, roleId, immuneRoleId, timespan.ToString(), mode == "Take", DateTime.MinValue, DateTime.UtcNow);
                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
