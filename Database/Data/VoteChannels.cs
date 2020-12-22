using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class VoteChannels
    {
        public static async Task<List<VoteChannelsRow>> GetRowsAsync(ulong? guildId = null, ulong? channelId = null, bool ignoreCache = false)
        {
            List<VoteChannelsRow> matchedRows = new List<VoteChannelsRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.VoteChannels.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
            }
            else
            {
                string command = "SELECT * FROM VoteChannels WHERE TRUE";
                List<(string, object)> values = new List<(string, object)>();

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

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(VoteChannelsRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetInt32(2),
                        reader.GetString(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<VoteChannelsRow> GetRowAsync(ulong guildId, ulong channelId)
        {
            var rows = await GetRowsAsync(guildId, channelId);
            return rows.Count > 0 ? rows.First() : new VoteChannelsRow(guildId, channelId);
        }

        public static async Task SaveRowAsync(VoteChannelsRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO VoteChannels (GuildId, ChannelId, Mode, Emotes) VALUES (@GuildId, @ChannelId, @Mode, @Emotes);",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("Mode", row.Mode),
                    ("Emotes", row.GetEmotesString()));

                row.New = false;
                if(Cache.Initialised) Cache.VoteChannels.Rows.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE VoteChannels SET Mode = @Mode, Emotes = @Emotes WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    ("GuildId", row.GuildId), 
                        ("ChannelId", row.ChannelId),
                        ("Mode", row.Mode),
                        ("Emotes", row.GetEmotesString()));

                if(Cache.Initialised) Cache.VoteChannels.Rows[Cache.VoteChannels.Rows.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static async Task DeleteRowAsync(VoteChannelsRow row)
        {
            if(Cache.Initialised) Cache.VoteChannels.Rows.RemoveAll(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId);

            await Sql.ExecuteAsync("DELETE FROM VoteChannels WHERE GuildId = @GuildId AND ChannelId = @ChannelId",
                ("GuildId", row.GuildId),
                ("ChannelId", row.ChannelId));
        }
    }

    public class VoteChannelsTable
    {
        public List<VoteChannelsRow> Rows { get; set; }
    }

    public class VoteChannelsRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public int Mode { get; set; }
        public List<IEmote> Emotes { get; set; }

        private VoteChannelsRow()
        {

        }

        public VoteChannelsRow(ulong guildId, ulong channelId)
        {
            New = true;
            GuildId = guildId;
            ChannelId = channelId;
            Mode = 0;
            Emotes = new List<IEmote>();
        }

        public static VoteChannelsRow FromDatabase(ulong guildId, ulong channelId, int mode, string emotes)
        {
            VoteChannelsRow row = new VoteChannelsRow
            {
                New = false,
                GuildId = guildId,
                ChannelId = channelId,
                Mode = mode,
                Emotes = new List<IEmote>()
            };

            emotes = EString.FromEncoded(emotes).Value;
            if (!string.IsNullOrEmpty(emotes))
            {
                foreach (string emoteString in emotes.Split(","))
                {
                    if (Emote.TryParse(emoteString, out Emote emote))
                    {
                        row.Emotes.Add(emote);
                    }
                    else
                    {
                        row.Emotes.Add(new Emoji(emoteString));
                    }
                }
            }

            return row;
        }

        public string GetEmotesString()
        {
            string emotesString = "";

            for (int i = 0; i < Emotes.Count; i++)
            {
                emotesString += Emotes[i].ToString();
                if (i != Emotes.Count - 1)
                {
                    emotesString += ",";
                }
            }

            return EString.FromDecoded(emotesString).EncodedValue;
        }
    }
}