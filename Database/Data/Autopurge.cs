using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Types
{
    public class Autopurge
    {
        public static List<AutopurgeRow> GetRowsWhere(ulong? guildId = null, ulong? channelId = null, TimeSpan? timeSpan = null, int? mode = null, int? messages = null)
        {
            List<AutopurgeRow> matchedRows = Cache.Autopurge.Rows;

            if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
            if (timeSpan.HasValue) matchedRows.RemoveAll(x => x.Timespan != timeSpan.Value);
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

        public void LoadAsync()
        // Load the table from the database
        {
            List<AutopurgeRow> newRows = new List<AutopurgeRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM Autopurge;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new AutopurgeRow(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetInt32(4),
                        reader.GetInt32(5)));
                }
            }
            catch {}

            Rows = newRows;
        }
    }

    public class AutopurgeRow
    {
        private int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public TimeSpan Timespan { get; set; }
        public int Mode { get; set; }
        public int Messages { get; set; }

        public AutopurgeRow(int id, string guildId, string channelId, string timespan, int mode, int messages)
        {
            Id = id;
            GuildId = ulong.Parse(guildId);
            ChannelId = ulong.Parse(channelId);
            Timespan = TimeSpan.Parse(timespan);
            Mode = mode;
            Messages = messages;
        }
    }
}