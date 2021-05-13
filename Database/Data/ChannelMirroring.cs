using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class ChannelMirroring
    {
        public static async Task<List<ChannelMirroringRow>> GetRowsAsync(ulong? guildId = null, ulong? fromChannelId = null, bool ignoreCache = false)
        {
            List<ChannelMirroringRow> matchedRows = new();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.ChannelMirroring);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (fromChannelId.HasValue) matchedRows.RemoveAll(x => x.FromChannelId != fromChannelId.Value);
            }
            else
            {
                string command = "SELECT * FROM ChannelMirroring WHERE TRUE";
                List<(string, object)> values = new();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                if (fromChannelId.HasValue)
                {
                    command += " AND FromChannelId = @FromChannelId";
                    values.Add(("FromChannelId", fromChannelId.Value));
                }

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(ChannelMirroringRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<ChannelMirroringRow> GetRowAsync(ulong guildId, ulong fromChannelId)
        {
            List<ChannelMirroringRow> rows = await GetRowsAsync(guildId, fromChannelId);
            return rows.Count > 0 ? rows.First() : new ChannelMirroringRow(guildId, fromChannelId);
        }

        public static async Task SaveRowAsync(ChannelMirroringRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync("INSERT INTO ChannelMirroring (GuildId, FromChannelId, ToChannelId, WebhookId) VALUES (@GuildId, @FromChannelId, @ToChannelId, @WebhookId);",
                    ("GuildId", row.GuildId), 
                    ("FromChannelId", row.FromChannelId),
                    ("ToChannelId", row.ToChannelId),
                    ("WebhookId", row.WebhookId));

                row.New = false;

                if(Cache.Initialised) Cache.ChannelMirroring.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE ChannelMirroring SET ToChannelId = @ToChannelId, WebhookId = @WebhookId WHERE GuildId = @GuildId AND FromChannelId = @FromChannelId;",
                    ("GuildId", row.GuildId), 
                        ("FromChannelId", row.FromChannelId),
                        ("ToChannelId", row.ToChannelId),
                        ("WebhookId", row.WebhookId));

                if(Cache.Initialised) Cache.ChannelMirroring[Cache.ChannelMirroring.FindIndex(x => x.GuildId == row.GuildId && x.FromChannelId == row.FromChannelId)] = row;
            }
        }

        public static async Task SaveWebhookIdAsync(ChannelMirroringRow row)
        {
            if (row.New)
            {
                await SaveRowAsync(row);
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE ChannelMirroring SET WebhookId = @WebhookId WHERE GuildId = @GuildId AND FromChannelId = @FromChannelId;",
                    ("GuildId", row.GuildId), 
                    ("FromChannelId", row.FromChannelId),
                    ("WebhookId", row.WebhookId));

                if(Cache.Initialised) Cache.ChannelMirroring[Cache.ChannelMirroring.FindIndex(x => x.GuildId == row.GuildId && x.FromChannelId == row.FromChannelId)] = row;
            }
        }

        public static async Task DeleteRowAsync(ChannelMirroringRow row)
        {
            if(Cache.Initialised) Cache.ChannelMirroring.RemoveAll(x => x.GuildId == row.GuildId && x.FromChannelId == row.FromChannelId);

            await Sql.ExecuteAsync("DELETE FROM ChannelMirroring WHERE GuildId = @GuildId AND FromChannelId = @FromChannelId",
                ("GuildId", row.GuildId),
                ("FromChannelId", row.FromChannelId));
        }
    }
    public class ChannelMirroringRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong FromChannelId { get; set; }
        public ulong ToChannelId { get; set; }
        public ulong WebhookId { get; set; }

        private ChannelMirroringRow()
        {

        }

        public ChannelMirroringRow(ulong guildId, ulong fromChannelId)
        {
            New = true;
            GuildId = guildId;
            FromChannelId = fromChannelId;
        }

        public static ChannelMirroringRow FromDatabase(ulong guildId, ulong fromChannelId, ulong toChannelId, ulong webhookId)
        {
            return new()
            {
                New = false,
                GuildId = guildId,
                FromChannelId = fromChannelId,
                ToChannelId = toChannelId,
                WebhookId = webhookId
            };
        }

        public async Task SaveAsync()
        {
            await ChannelMirroring.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await ChannelMirroring.DeleteRowAsync(this);
        }
    }
}
