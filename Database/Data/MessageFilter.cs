using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Data
{
    public static class MessageFilter
    {
        public static async Task<List<MessageFilterRow>> GetRowsAsync(ulong? guildId = null, ulong? channelId = null, bool ignoreCache = false)
        {
            var matchedRows = new List<MessageFilterRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.MessageFilter);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
            }
            else
            {
                var command = "SELECT * FROM MessageFilter WHERE TRUE";
                var values = new List<(string, object)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                if (channelId.HasValue)
                {
                    command += " AND ChannelId = @ChannelId";
                    values.Add(("ChannelId", channelId.Value));
                }

                var reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(MessageFilterRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetInt32(2),
                        reader.GetString(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<MessageFilterRow> GetRowAsync(ulong guildId, ulong channelId)
        {
            var rows = await GetRowsAsync(guildId, channelId);
            return rows.Count > 0 ? rows.First() : new MessageFilterRow(guildId, channelId);
        }

        public static async Task SaveRowAsync(MessageFilterRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync("INSERT INTO MessageFilter (GuildId, ChannelId, Mode, Complex) VALUES (@GuildId, @ChannelId, @Mode, @Complex);",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("Mode", row.Mode),
                    ("Complex", row.Complex.EncodedValue));

                row.New = false;
                if(Cache.Initialised) Cache.MessageFilter.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE MessageFilter SET Mode = @Mode, Complex = @Complex WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("Mode", row.Mode),
                    ("Complex", row.Complex.EncodedValue));

                if(Cache.Initialised) Cache.MessageFilter[Cache.MessageFilter.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static async Task DeleteRowAsync(MessageFilterRow row)
        {
            if(Cache.Initialised) Cache.MessageFilter.RemoveAll(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId);

            await Sql.ExecuteAsync("DELETE FROM MessageFilter WHERE GuildId = @GuildId AND ChannelId = @ChannelId",
                ("GuildId", row.GuildId),
                ("ChannelId", row.ChannelId));
        }
    }
    public class MessageFilterRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public int Mode { get; set; }
        // 0    All messages
        // 1    Images
        // 2    Videos
        // 3    Media
        // 4    Music
        // 5    Attachments
        // 6    URLs
        // 7    URLs and Media
        // 8    RegEx

        public EString Complex { get; set; }

        private MessageFilterRow()
        {

        }

        public MessageFilterRow(ulong guildId, ulong channelId)
        {
            New = true;
            GuildId = guildId;
            ChannelId = channelId;
            Mode = 0;
            Complex = EString.Empty;
        }

        public static MessageFilterRow FromDatabase(ulong guildId, ulong channelId, int mode, string complex)
        {
            return new()
            {
                New = false,
                GuildId = guildId,
                ChannelId = channelId,
                Mode = mode,
                Complex = EString.FromEncoded(complex)
            };
        }

        public async Task SaveAsync()
        {
            await MessageFilter.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await MessageFilter.DeleteRowAsync(this);
        }
    }
}
