using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class ChannelMirroring
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> links;
            if(oneGuildId == null) links = V1Data.GetDataList(type: "Mirroring-Link");
            else links = V1Data.GetDataList(oneGuildId.ToString(), "Mirroring-Link");

            foreach (V1Data v1Link in links)
            {
                try
                {
                    if (!v1Link.Value.Contains("G"))
                    {
                        ulong guildId = ulong.Parse(v1Link.GuildId);
                        ulong fromChannelId = ulong.Parse(v1Link.Value.Split(" -> ").First());
                        ulong toChannelId = ulong.Parse(v1Link.Value.Split(" -> ").Last());

                        ulong webhookId = 0;
                        try { webhookId = ulong.Parse(V1Data.GetFirstData(guildId.ToString(), $"Mirroring-WebhookID-{toChannelId}").Value); } catch { }

                        ChannelMirroringRow row = ChannelMirroringRow.FromDatabase(guildId, fromChannelId, toChannelId, webhookId);
                        Program.RowsToSave.Add(row);
                    }
                }
                catch { }
            }
        }
    }
}
