using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class VoiceRoles
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> channels;
            if (oneGuildId == null) channels = V1Data.GetDataWhere("DataType LIKE '%VCRoles-Role-%'");
            else channels = V1Data.GetDataWhere($"GuildID = '{oneGuildId}' AND DataType LIKE '%VCRoles-Role-%'");

            foreach (V1Data v1Channel in channels)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1Channel.GuildId);
                    ulong channelId = ulong.Parse(v1Channel.Type.Split("-").Last());
                    ulong roleId = ulong.Parse(v1Channel.Value);

                    VoiceRolesRow row = new VoiceRolesRow(guildId, channelId)
                    {
                        RoleId = roleId
                    };

                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
