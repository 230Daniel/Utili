using System.Collections.Generic;
using System.Linq;
using Discord;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class VoteChannels
    {
        public static List<VoteChannelsRow> GetRows(ulong? guildId = null, ulong? channelId = null, bool ignoreCache = false)
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
                List<(string, string)> values = new List<(string, string)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                if (channelId.HasValue)
                {
                    command += " AND ChannelId = @ChannelId";
                    values.Add(("ChannelId", channelId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

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

        public static void SaveRow(VoteChannelsRow row)
        {
            MySqlCommand command;

            if (row.New)
            {
                command = Sql.GetCommand("INSERT INTO VoteChannels (GuildId, ChannelId, Mode, Emotes) VALUES (@GuildId, @ChannelId, @Mode, @Emotes);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Mode", row.Mode.ToString()),
                        ("Emotes", row.GetEmotesString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;

                if(Cache.Initialised) Cache.VoteChannels.Rows.Add(row);
            }
            else
            {
                command = Sql.GetCommand("UPDATE VoteChannels SET Mode = @Mode, Emotes = @Emotes WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Mode", row.Mode.ToString()),
                        ("Emotes", row.GetEmotesString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.VoteChannels.Rows[Cache.VoteChannels.Rows.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static void DeleteRow(VoteChannelsRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.VoteChannels.Rows.RemoveAll(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId);

            string commandText = "DELETE FROM VoteChannels WHERE GuildId = @GuildId AND ChannelId = @ChannelId";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {
                    ("GuildId", row.GuildId.ToString()),
                    ("ChannelId", row.ChannelId.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
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

        public VoteChannelsRow()
        {
            New = true;
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