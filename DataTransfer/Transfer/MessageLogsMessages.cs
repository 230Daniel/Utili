using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.Data;

namespace DataTransfer.Transfer
{
    internal static class MessageLogsMessages
    {
        public static async Task TransferAsync(ulong? oneGuildId = null)
        {
            List<V1MessageData> v1Rows;
            if(oneGuildId == null) v1Rows = await V1Data.GetMessagesAsync();
            else v1Rows = await V1Data.GetMessagesAsync(oneGuildId.Value);

            foreach (V1MessageData message in v1Rows)
            {
                try
                {
                    string content = V1Data.Decrypt(message.EncryptedContent, ulong.Parse(message.GuildId), ulong.Parse(message.ChannelId));
                    MessageLogsMessageRow row = MessageLogsMessageRow.FromDatabase(ulong.Parse(message.GuildId), ulong.Parse(message.ChannelId),
                        ulong.Parse(message.MessageId), ulong.Parse(message.UserId), message.Timestmap, "");
                    row.Content = EString.FromDecoded(content);

                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
