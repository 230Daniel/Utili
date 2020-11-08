using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Database.Data
{
    public class VoteChannels
    {
        public static List<VoteChannelsRow> GetRows(ulong? guildId = null, ulong? channelId = null, int? id = null, bool ignoreCache = false)
        {
            List<VoteChannelsRow> matchedRows = new List<VoteChannelsRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.VoteChannels.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
                if (id.HasValue) matchedRows.RemoveAll(x => x.Id != id.Value);
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

                if (id.HasValue)
                {
                    command += " AND Id = @Id";
                    values.Add(("Id", id.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(new VoteChannelsRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetInt32(3),
                        reader.GetString(4)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static void SaveRow(VoteChannelsRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO VoteChannels (GuildID, ChannelId, Mode, Emotes) VALUES (@GuildId, @ChannelId, @Mode, @Emotes);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Mode", row.Mode.ToString()),
                        ("Emotes", row.GetEmotesString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.Id = GetRows(row.GuildId, row.ChannelId, ignoreCache: true).First().Id;

                if(Cache.Initialised) Cache.VoteChannels.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE VoteChannels SET GuildId = @GuildId, ChannelId = @ChannelId, Mode = @Mode, Emotes = @Emotes WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("Mode", row.Mode.ToString()),
                        ("Emotes", row.GetEmotesString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.VoteChannels.Rows[Cache.VoteChannels.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void DeleteRow(VoteChannelsRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.VoteChannels.Rows.RemoveAll(x => x.Id == row.Id);

            string commandText = "DELETE FROM VoteChannels WHERE Id = @Id";
            MySqlCommand command = Sql.GetCommand(commandText, new[] {("Id", row.Id.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class VoteChannelsTable
    {
        public List<VoteChannelsRow> Rows { get; set; }

        public void Load()
        // Load the table from the database
        {
            List<VoteChannelsRow> newRows = new List<VoteChannelsRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM VoteChannels;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new VoteChannelsRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetInt32(3),
                        reader.GetString(4)));
                }
            }
            catch {}

            reader.Close();

            Rows = newRows;
        }
    }

    public class VoteChannelsRow
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public int Mode { get; set; }
        public List<IEmote> Emotes { get; set; }

        public VoteChannelsRow()
        {
            Id = 0;
        }

        public VoteChannelsRow(int id, ulong guildId, ulong channelId, int mode, string emotes)
        {
            Id = id;
            GuildId = guildId;
            ChannelId = channelId;
            Mode = mode;

            Emotes = new List<IEmote>();

            emotes = EString.FromEncoded(emotes).Value;

            if (!string.IsNullOrEmpty(emotes))
            {
                foreach (string emoteString in emotes.Split(","))
                {
                    if (Emote.TryParse(emoteString, out Emote emote))
                    {
                        Emotes.Add(emote);
                    }
                    else
                    {
                        Emotes.Add(new Emoji(emoteString));
                    }
                }
            }
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