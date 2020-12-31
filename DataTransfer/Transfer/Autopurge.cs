using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class Autopurge
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> channels;
            if(oneGuildId == null) channels = V1Data.GetDataList(type: "Autopurge-Channel");
            else channels = V1Data.GetDataList(oneGuildId.ToString(), "Autopurge-Channel");

            foreach (V1Data v1Channel in channels)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1Channel.GuildId);
                    ulong channelId = ulong.Parse(v1Channel.Value);

                    TimeSpan time = TimeSpan.FromMinutes(15);
                    try { time = TimeSpan.Parse(V1Data.GetFirstData(guildId.ToString(), $"Autopurge-Timespan-{channelId}").Value); } catch { }

                    int mode = 0;
                    if (V1Data.DataExists(guildId.ToString(), $"Autopurge-Mode-{channelId}", "Bots")) mode = 1;

                    
                    AutopurgeRow row = AutopurgeRow.FromDatabase(guildId, channelId, time.ToString(), mode);
                    row.New = true;
                    await Database.Data.Autopurge.SaveRowAsync(row);
                    Console.WriteLine($"Done {row.GuildId}-{row.ChannelId}");
                }
                catch { }
            }
        }
    }
}
