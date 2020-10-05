using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Types
{
    public class Autopurge
    {
        public List<AutopurgeRow> GetRowsWhere(ulong? guildId = null, ulong? channelId = null, TimeSpan? timeSpan = null, int? mode = null, int? messages = null)
        {
            List<AutopurgeRow> matchedRows = Cache.Autopurge.Rows;

            if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
            if (timeSpan.HasValue) matchedRows.RemoveAll(x => x.TimeSpan != timeSpan.Value);
            if (mode.HasValue) matchedRows.RemoveAll(x => x.Mode != mode.Value);
            if (messages.HasValue) matchedRows.RemoveAll(x => x.Messages != messages.Value);

            return matchedRows;
        }

        public List<AutopurgeRow> GetRowsForGuilds(List<ulong> guilds)
        {
            return Cache.Autopurge.Rows.Where(x => guilds.Contains(x.GuildId)).ToList();
        }
    }

    public class AutopurgeTable
    {
        public List<AutopurgeRow> Rows { get; set; }

        public async Task LoadAsync()
        // Load the table from the database asyncronously
        {
            // Simulate data load
            Rows.Clear();

            Rows.Add(new AutopurgeRow
                {
                    GuildId = 0, 
                    ChannelId = 0, 
                    TimeSpan = TimeSpan.FromSeconds(5), 
                    Mode = 0, 
                    Messages = 10,
                });

            // Simulate database latency
            await Task.Delay(23);
        }
    }

    public class AutopurgeRow
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public int Mode { get; set; }
        public int Messages { get; set; }
    }
}