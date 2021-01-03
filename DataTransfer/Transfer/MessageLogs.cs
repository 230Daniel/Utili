using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database.Data;
using Discord.Rest;

namespace DataTransfer.Transfer
{
    internal static class MessageLogs
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> channels;
            if(oneGuildId == null) channels = V1Data.GetDataList(type: "MessageLogs-LogChannel");
            else channels = V1Data.GetDataList(oneGuildId.ToString(), "MessageLogs-LogChannel");

            foreach (V1Data v1Channel in channels)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1Channel.GuildId);
                    ulong logChannelId = ulong.Parse(v1Channel.Value);

                    RestGuild guild = await Program.Client.GetGuildAsync(guildId);
                    List<ulong> guildChannels = (await guild.GetTextChannelsAsync()).Select(x => x.Id).ToList();
                    List<ulong> included = V1Data.GetDataList(guildId.ToString(), "MessageLogs-Channel").Select(x => ulong.Parse(x.Value)).ToList();

                    List<ulong> excluded = guildChannels.Where(x => !included.Contains(x)).ToList();

                    MessageLogsRow row = MessageLogsRow.FromDatabase(guildId, logChannelId, logChannelId, "");
                    row.ExcludedChannels = excluded;
                    Program.RowsToSave.Add(row);

                    await Task.Delay(334);
                }
                catch { }
            }
        }
    }
}
