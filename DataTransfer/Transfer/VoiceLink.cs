using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;
using Discord.Rest;

namespace DataTransfer.Transfer
{
    internal static class VoiceLink
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> guilds;
            if(oneGuildId == null) guilds = V1Data.GetDataList(type: "VCLink-Enabled");
            else guilds = V1Data.GetDataList(oneGuildId.ToString(), "VCLink-Enabled");

            foreach (V1Data v1Guild in guilds)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1Guild.GuildId);
                    List<ulong> excluded = V1Data.GetDataList(guildId.ToString(), "VCLink-Exclude").Select(x => ulong.Parse(x.Value)).ToList();

                    VoiceLinkRow row = new VoiceLinkRow(guildId)
                    {
                        DeleteChannels = true,
                        Enabled = true,
                        Prefix = EString.FromDecoded("vc-"),
                        ExcludedChannels = excluded
                    };
                    Program.RowsToSave.Add(row);
                }
                catch { }
            }

            List<V1Data> channels;
            if (oneGuildId == null) channels = V1Data.GetDataWhere("DataType LIKE '%VCLink-Channel-%'");
            else channels = V1Data.GetDataWhere($"GuildID = '{oneGuildId}' AND DataType LIKE '%VCLink-Channel-%'");

            foreach (V1Data v1Channel in channels)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1Channel.GuildId);
                    ulong voiceChannelId = ulong.Parse(v1Channel.Type.Split("-").Last());
                    ulong textChannelId = ulong.Parse(v1Channel.Value);

                    VoiceLinkChannelRow row = new VoiceLinkChannelRow(guildId, voiceChannelId)
                    {
                        TextChannelId = textChannelId
                    };

                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
