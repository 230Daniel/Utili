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

            List<(ulong, int)> messagesPerChannel = new List<(ulong, int)>();

            foreach (V1MessageData message in v1Rows)
            {
                try
                {
                    ulong channelId = ulong.Parse(message.ChannelId);

                    if (messagesPerChannel.Any(x => x.Item1 == channelId))
                    {
                        (ulong, int) record = messagesPerChannel.First(x => x.Item1 == channelId);
                        if (record.Item2 >= 50) continue;
                        record.Item2 += 1;
                    }
                    else
                    {
                        messagesPerChannel.Add((channelId, 1));
                    }

                    ulong guildId = ulong.Parse(message.GuildId);
                    ulong messageId = ulong.Parse(message.MessageId);
                    ulong userId = ulong.Parse(message.UserId);
                    string content = V1Data.Decrypt(message.EncryptedContent, guildId, channelId);

                    MessageLogsMessageRow row = MessageLogsMessageRow.FromDatabase(guildId, channelId,
                        messageId, userId, message.Timestmap, "");
                    row.Content = EString.FromDecoded(content);

                    Program.RowsToSave.Add(row);
                }
                catch { }
            }
        }
    }
}
