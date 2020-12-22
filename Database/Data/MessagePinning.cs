using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class MessagePinning
    {
        public static async Task<List<MessagePinningRow>> GetRowsAsync(ulong? guildId = null, bool ignoreCache = false)
        {
            List<MessagePinningRow> matchedRows = new List<MessagePinningRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.MessagePinning.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM MessagePinning WHERE TRUE";
                List<(string, object)> values = new List<(string, object)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(MessagePinningRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetBoolean(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<MessagePinningRow> GetRowAsync(ulong guildId)
        {
            List<MessagePinningRow> rows = await GetRowsAsync(guildId);
            return rows.Count > 0 ? rows.First() : new MessagePinningRow(guildId);
        }

        public static async Task SaveRowAsync(MessagePinningRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO MessagePinning (GuildId, PinChannelId, WebhookId, Pin) VALUES (@GuildId, @PinChannelId, @WebhookId, @Pin);", 
                    ("GuildId", row.GuildId), 
                    ("PinChannelId", row.PinChannelId),
                    ("WebhookId", row.WebhookId),
                    ("Pin", row.Pin));

                row.New = false;
                if(Cache.Initialised) Cache.MessagePinning.Rows.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE MessagePinning SET PinChannelId = @PinChannelId, WebhookId = @WebhookId, Pin = @Pin WHERE GuildId = @GuildId;",
                    ("GuildId", row.GuildId), 
                    ("PinChannelId", row.PinChannelId),
                    ("WebhookId", row.WebhookId),
                    ("Pin", row.Pin));

                if(Cache.Initialised) Cache.MessagePinning.Rows[Cache.MessagePinning.Rows.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static async Task DeleteRowAsync(MessagePinningRow row)
        {
            if(Cache.Initialised) Cache.MessagePinning.Rows.RemoveAll(x => x.GuildId == row.GuildId);

            await Sql.ExecuteAsync(
                "DELETE FROM MessagePinning WHERE GuildId = @GuildId",
                ("GuildId", row.GuildId));
        }
    }

    public class MessagePinningTable
    {
        public List<MessagePinningRow> Rows { get; set; }
    }

    public class MessagePinningRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong PinChannelId { get; set; }
        public ulong WebhookId { get; set; }
        public bool Pin { get; set; }

        private MessagePinningRow()
        {
        }

        public MessagePinningRow(ulong guildId)
        {
            New = true;
            GuildId = guildId;
            PinChannelId = 0;
            Pin = false;
        }

        public static MessagePinningRow FromDatabase(ulong guildId, ulong pinChannelId, ulong webhookId, bool pin)
        {
            return new MessagePinningRow
            {
                New = false,
                GuildId = guildId,
                PinChannelId = pinChannelId,
                WebhookId = webhookId,
                Pin = pin
            };
        }
    }
}
