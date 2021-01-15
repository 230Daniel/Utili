using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class RolePersist
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> rolePersists;
            if(oneGuildId == null) rolePersists = V1Data.GetDataList(type: "RolePersist-Enabled");
            else rolePersists = V1Data.GetDataList(oneGuildId.ToString(), "RolePersist-Enabled");

            foreach (ulong guildId in rolePersists.Select(x => ulong.Parse(x.GuildId)).Distinct())
            {
                try
                {
                    RolePersistRow row = new RolePersistRow(guildId)
                    {
                        Enabled = true
                    };
                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
