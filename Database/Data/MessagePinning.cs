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
                matchedRows.AddRange(Cache.MessagePinning);

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
                        reader.GetString(2),
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
                    "INSERT INTO MessagePinning (GuildId, PinChannelId, WebhookIds, Pin) VALUES (@GuildId, @PinChannelId, @WebhookIds, @Pin);", 
                    ("GuildId", row.GuildId),
                    ("PinChannelId", row.PinChannelId),
                    ("WebhookIds", row.GetWebhookIdsString()),
                    ("Pin", row.Pin));

                row.New = false;
                if(Cache.Initialised) Cache.MessagePinning.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE MessagePinning SET PinChannelId = @PinChannelId, WebhookIds = @WebhookIds, Pin = @Pin WHERE GuildId = @GuildId;",
                    ("GuildId", row.GuildId), 
                    ("PinChannelId", row.PinChannelId),
                    ("WebhookIds", row.GetWebhookIdsString()),
                    ("Pin", row.Pin));

                if(Cache.Initialised) Cache.MessagePinning[Cache.MessagePinning.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static async Task DeleteRowAsync(MessagePinningRow row)
        {
            if(Cache.Initialised) Cache.MessagePinning.RemoveAll(x => x.GuildId == row.GuildId);

            await Sql.ExecuteAsync(
                "DELETE FROM MessagePinning WHERE GuildId = @GuildId",
                ("GuildId", row.GuildId));
        }
    }
    public class MessagePinningRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong PinChannelId { get; set; }
        public List<(ulong, ulong)> WebhookIds { get; set; }
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
            WebhookIds = new List<(ulong, ulong)>();
        }

        public static MessagePinningRow FromDatabase(ulong guildId, ulong pinChannelId, string webhookIds, bool pin)
        {
            MessagePinningRow row = new MessagePinningRow
            {
                New = false,
                GuildId = guildId,
                PinChannelId = pinChannelId,
                WebhookIds = new List<(ulong, ulong)>(),
                Pin = pin
            };

            webhookIds = EString.FromEncoded(webhookIds).Value;
            if (!string.IsNullOrEmpty(webhookIds))
            {
                foreach (string emoteString in webhookIds.Split(","))
                {
                    ulong channelId = ulong.Parse(emoteString.Split("///").First());
                    ulong webhookId = ulong.Parse(emoteString.Split("///").Last());
                    row.WebhookIds.Add((channelId, webhookId));
                }
            }

            return row;
        }

        public string GetWebhookIdsString()
        {
            string idsString = "";

            for (int i = 0; i < WebhookIds.Count; i++)
            {
                idsString += $"{WebhookIds[i].Item1}///{WebhookIds[i].Item2}";
                if (i != WebhookIds.Count - 1)
                {
                    idsString += ",";
                }
            }

            return EString.FromDecoded(idsString).EncodedValue;
        }

        public async Task SaveAsync()
        {
            await MessagePinning.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await MessagePinning.DeleteRowAsync(this);
        }
    }
}
