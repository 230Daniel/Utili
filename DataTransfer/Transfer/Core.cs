using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class Core
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1Data> prefixes;
            if(oneGuildId == null) prefixes = V1Data.GetDataList(type: "Prefix");
            else prefixes = V1Data.GetDataList(oneGuildId.ToString(), "Prefix");

            List<V1Data> disabledChannels;
            if(oneGuildId == null) disabledChannels = V1Data.GetDataList(type: "Commands-Disabled");
            else disabledChannels = V1Data.GetDataList(oneGuildId.ToString(), "Commands-Disabled");

            foreach (V1Data v1Prefix in prefixes)
            {
                try
                {
                    ulong guildId = ulong.Parse(v1Prefix.GuildId);
                    CoreRow row = new CoreRow(guildId)
                    {
                        Prefix = EString.FromDecoded(v1Prefix.Value),
                        EnableCommands = true,
                        ExcludedChannels = disabledChannels.Where(x => x.GuildId == guildId.ToString()).Select(x => ulong.Parse(x.Value)).ToList()
                    };
                    Program.RowsToSave.Add(row);
                }
                catch { }
            }

            foreach (ulong guildId in disabledChannels.Select(x => ulong.Parse(x.GuildId)).Distinct().Where(x => prefixes.All(y => y.GuildId != x.ToString())))
            {
                try
                {
                    CoreRow row = new CoreRow(guildId)
                    {
                        Prefix = EString.FromDecoded("."),
                        EnableCommands = true,
                        ExcludedChannels = disabledChannels.Where(x => x.GuildId == guildId.ToString()).Select(x => ulong.Parse(x.Value)).ToList()
                    };
                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
