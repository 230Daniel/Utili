using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public class VoiceLink
    {
        #region Meta Rows

        public static List<VoiceLinkRow> GetMetaRows(ulong? guildId = null, bool ignoreCache = false)
        {
            List<VoiceLinkRow> matchedRows = new List<VoiceLinkRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.VoiceLink.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM VoiceLink WHERE TRUE";
                List<(string, string)> values = new List<(string, string)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(new VoiceLinkRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetBoolean(2),
                        reader.GetBoolean(3),
                        reader.GetString(4),
                        reader.GetString(5)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static VoiceLinkRow GetMetaRow(ulong guildId)
        {
            List<VoiceLinkRow> rows = GetMetaRows(guildId);

            if (rows.Count == 0)
            {
                return new VoiceLinkRow
                {
                    Id = 0,
                    GuildId = guildId,
                    Enabled = false,
                    DeleteChannels = true,
                    Prefix = EString.FromDecoded("vc-")
                };
            }
            
            return rows.First();
        }

        public static void SaveMetaRow(VoiceLinkRow metaRow)
        {
            MySqlCommand command;

            if (metaRow.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand($"INSERT INTO VoiceLink (GuildID, Enabled, DeleteChannels, Prefix, ExcludedChannels) VALUES (@GuildId, {Sql.ToSqlBool(metaRow.Enabled)}, {Sql.ToSqlBool(metaRow.DeleteChannels)}, @Prefix, @ExcludedChannels );",
                    new [] {("GuildId", metaRow.GuildId.ToString()), 
                        ("Prefix", metaRow.Prefix.EncodedValue),
                        ("ExcludedChannels", metaRow.GetExcludedChannelsString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
                metaRow.Id = GetMetaRows(metaRow.GuildId, true).First().Id;
                
                if(Cache.Initialised) Cache.VoiceLink.Rows.Add(metaRow);
            }
            else 
            // The row already exists and should be updated
            {
                command = Sql.GetCommand($"UPDATE VoiceLink SET GuildId = @GuildId, Enabled = {Sql.ToSqlBool(metaRow.Enabled)}, DeleteChannels = {Sql.ToSqlBool(metaRow.DeleteChannels)}, Prefix = @Prefix, ExcludedChannels = @ExcludedChannels WHERE Id = @Id;",
                    new [] {("Id", metaRow.Id.ToString()),
                        ("GuildId", metaRow.GuildId.ToString()), 
                        ("Prefix", metaRow.Prefix.EncodedValue),
                        ("ExcludedChannels", metaRow.GetExcludedChannelsString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.VoiceLink.Rows[Cache.VoiceLink.Rows.FindIndex(x => x.Id == metaRow.Id)] = metaRow;
            }
        }

        #endregion

        #region Channel Rows

        public static List<VoiceLinkChannelRow> GetChannelRows(ulong? guildId = null, ulong? voiceChannelId = null, bool ignoreCache = false)
        {
            List<VoiceLinkChannelRow> matchedRows = new List<VoiceLinkChannelRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.VoiceLink.Channels);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (voiceChannelId.HasValue) matchedRows.RemoveAll(x => x.VoiceChannelId != voiceChannelId.Value);
            }
            else
            {
                string command = "SELECT * FROM VoiceLinkChannels WHERE TRUE";
                List<(string, string)> values = new List<(string, string)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                if (voiceChannelId.HasValue)
                {
                    command += " AND VoiceChannelId = @VoiceChannelId";
                    values.Add(("VoiceChannelId", voiceChannelId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(new VoiceLinkChannelRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static VoiceLinkChannelRow GetChannelRow(ulong guildId, ulong voiceChannelId)
        {
            List<VoiceLinkChannelRow> rows = GetChannelRows(guildId, voiceChannelId);

            if (rows.Count == 0)
            {
                return new VoiceLinkChannelRow(0, guildId, 0, voiceChannelId);
            }
            
            return rows.First();
        }

        public static void SaveChannelRow(VoiceLinkChannelRow channelRow)
        {
            MySqlCommand command;

            if (channelRow.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO VoiceLinkChannels (GuildID, TextChannelId, VoiceChannelId) VALUES (@GuildId, @TextChannelId, @VoiceChannelId);",
                    new [] { ("GuildId", channelRow.GuildId.ToString()), 
                        ("TextChannelId", channelRow.TextChannelId.ToString()),
                        ("VoiceChannelId", channelRow.VoiceChannelId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();
                channelRow.Id = GetChannelRows(channelRow.GuildId, channelRow.VoiceChannelId, true).First().Id;
                
                if(Cache.Initialised) Cache.VoiceLink.Channels.Add(channelRow);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE VoiceLinkChannels SET GuildId = @GuildId, TextChannelId = @TextChannelId, VoiceChannelId = @VoiceChannelId WHERE Id = @Id;",
                    new [] {("Id", channelRow.Id.ToString()),
                        ("GuildId", channelRow.GuildId.ToString()), 
                        ("TextChannelId", channelRow.TextChannelId.ToString()),
                        ("VoiceChannelId", channelRow.VoiceChannelId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.VoiceLink.Channels[Cache.VoiceLink.Channels.FindIndex(x => x.Id == channelRow.Id)] = channelRow;
            }
        }

        #endregion
    }

    public class VoiceLinkTable
    {
        public List<VoiceLinkRow> Rows { get; set; }
        public List<VoiceLinkChannelRow> Channels { get; set; }

        public void Load()
        // Load the table from the database
        {
            #region Meta Rows

            List<VoiceLinkRow> newRows = new List<VoiceLinkRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM VoiceLink;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new VoiceLinkRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetBoolean(2),
                        reader.GetBoolean(3),
                        reader.GetString(4),
                        reader.GetString(5)));
                }
            }
            catch {}

            reader.Close();
            Rows = newRows;

            #endregion

            #region Channel Rows

            List<VoiceLinkChannelRow> newChannelRows = new List<VoiceLinkChannelRow>();

            MySqlDataReader channelReader = Sql.GetCommand("SELECT * FROM VoiceLinkChannels;").ExecuteReader();

            try
            {
                while (channelReader.Read())
                {
                    newChannelRows.Add(new VoiceLinkChannelRow(
                        channelReader.GetInt32(0),
                        channelReader.GetUInt64(1),
                        channelReader.GetUInt64(2),
                        channelReader.GetUInt64(3)));
                }
            }
            catch {}

            channelReader.Close();
            Channels = newChannelRows;

            #endregion
        }
    }

    public class VoiceLinkRow
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public bool Enabled { get; set; }
        public bool DeleteChannels { get; set; }
        public EString Prefix { get; set; }
        public List<ulong> ExcludedChannels { get; set; }

        public VoiceLinkRow()
        {
            Id = 0;
        }

        public VoiceLinkRow(int id, ulong guildId, bool enabled, bool deleteChannels, string prefix, string excludedChannels)
        {
            Id = id;
            GuildId = guildId;
            Enabled = enabled;
            DeleteChannels = deleteChannels;
            Prefix = EString.FromEncoded(prefix);

            ExcludedChannels = new List<ulong>();

            if (!string.IsNullOrEmpty(excludedChannels))
            {
                foreach (string excludedChannel in excludedChannels.Split(","))
                {
                    if (ulong.TryParse(excludedChannel, out ulong channelId))
                    {
                        ExcludedChannels.Add(channelId);
                    }
                }
            }
        }

        public string GetExcludedChannelsString()
        {
            string excludedChannelsString = "";

            for (int i = 0; i < ExcludedChannels.Count; i++)
            {
                ulong excludedChannelId = ExcludedChannels[i];
                excludedChannelsString += excludedChannelId.ToString();
                if (i != ExcludedChannels.Count - 1)
                {
                    excludedChannelsString += ",";
                }
            }

            return excludedChannelsString;
        }
    }

    public class VoiceLinkChannelRow
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong TextChannelId { get; set; }
        public ulong VoiceChannelId { get; set; }

        public VoiceLinkChannelRow()
        {
            Id = 0;
        }

        public VoiceLinkChannelRow(int id, ulong guildId, ulong textChannelId, ulong voiceChannelId)
        {
            Id = id;
            GuildId = guildId;
            TextChannelId = textChannelId;
            VoiceChannelId = voiceChannelId;
        }
    }
}