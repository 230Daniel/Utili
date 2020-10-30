﻿using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Database.Data
{
    public class VoiceLink
    {
        public static List<VoiceLinkRow> GetRows(ulong? guildId = null, ulong? voiceChannelId = null, bool ignoreCache = false)
        {
            List<VoiceLinkRow> matchedRows = new List<VoiceLinkRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.VoiceLink.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (voiceChannelId.HasValue) matchedRows.RemoveAll(x => x.VoiceChannelId != voiceChannelId.Value);
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

                if (voiceChannelId.HasValue)
                {
                    command += " AND VoiceChannelId = @VoiceChannelId";
                    values.Add(("VoiceChannelId", voiceChannelId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(new VoiceLinkRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3),
                        reader.GetBoolean(4),
                        reader.GetBoolean(5),
                        reader.GetString(6)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static VoiceLinkRow GetRowForChannel(ulong guildId, ulong voiceChannelId)
        {
            List<VoiceLinkRow> rows = GetRows(guildId, voiceChannelId);

            if (rows.Count == 0)
            {
                return new VoiceLinkRow(0, guildId, 0, voiceChannelId, false, false, "");
            }
            
            return rows.First();
        }

        public static VoiceLinkRow GetMetaRow(ulong guildId)
        {
            List<VoiceLinkRow> rows = GetRows(guildId, 0);

            if (rows.Count == 0)
            {
                return new VoiceLinkRow(0, guildId, 0, 0, false, false, "vc-");
            }
            
            return rows.First();
        }

        public static void SaveRow(VoiceLinkRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand($"INSERT INTO VoiceLink (GuildID, TextChannelId, VoiceChannelId, Excluded, Enabled, Prefix) VALUES (@GuildId, @TextChannelId, @VoiceChannelId, {Sql.GetBool(row.Excluded)}, {Sql.GetBool(row.Enabled)}, @Prefix);",
                    new [] { ("GuildId", row.GuildId.ToString()), 
                        ("TextChannelId", row.TextChannelId.ToString()),
                        ("VoiceChannelId", row.VoiceChannelId.ToString()),
                        ("Prefix", row.Prefix)});

                command.ExecuteNonQuery();
                command.Connection.Close();
                row.Id = GetRows(row.GuildId, row.VoiceChannelId, true).First().Id;
                
                if(Cache.Initialised) Cache.VoiceLink.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand($"UPDATE VoiceLink SET GuildId = @GuildId, TextChannelId = @TextChannelId, VoiceChannelId = @VoiceChannelId, Excluded = {Sql.GetBool(row.Excluded)}, Enabled = {Sql.GetBool(row.Enabled)}, Prefix = @Prefix WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("TextChannelId", row.TextChannelId.ToString()),
                        ("VoiceChannelId", row.VoiceChannelId.ToString()),
                        ("Prefix", row.Prefix)});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.VoiceLink.Rows[Cache.VoiceLink.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void SaveTextChannel(VoiceLinkRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so there's no need to protect existing values
            {
                SaveRow(row);
            }
            else
            // The row already exists so another value could possibly have been changed
            {
                command = Sql.GetCommand($"UPDATE VoiceLink SET TextChannelId = @TextChannelId WHERE Id = @Id;",
                    new[]
                    {
                        ("Id", row.Id.ToString()),
                        ("TextChannelId", row.TextChannelId.ToString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.VoiceLink.Rows[Cache.VoiceLink.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void DeleteRow(VoiceLinkRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.VoiceLink.Rows.RemoveAll(x => x.Id == row.Id);

            string commandText = "DELETE FROM VoiceLink WHERE Id = @Id";
            MySqlCommand command = Sql.GetCommand(commandText, new[] {("Id", row.Id.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }

        public static void DeleteUnrequiredRows()
        {
            Cache.VoiceLink.Rows.RemoveAll(x =>
                x.TextChannelId == 0 && x.VoiceChannelId != 0 && !x.Excluded);

            string commandText =
                "DELETE FROM VoiceLink WHERE TextChannelId = '0' AND VoiceChannelID != '0' AND Excluded = FALSE;";

            MySqlCommand command = Sql.GetCommand(commandText);
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class VoiceLinkTable
    {
        public List<VoiceLinkRow> Rows { get; set; }

        public void Load()
        // Load the table from the database
        {
            List<VoiceLinkRow> newRows = new List<VoiceLinkRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM VoiceLink;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new VoiceLinkRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3),
                        reader.GetBoolean(4),
                        reader.GetBoolean(5),
                        reader.GetString(6)));
                }
            }
            catch {}

            reader.Close();
            Rows = newRows;
        }
    }

    public class VoiceLinkRow
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong TextChannelId { get; set; }
        public ulong VoiceChannelId { get; set; }
        public bool Excluded { get; set; }
        public bool Enabled { get; set; }
        public string Prefix{ get; set; }

        public VoiceLinkRow()
        {
            Id = 0;
        }

        public VoiceLinkRow(int id, ulong guildId, ulong textChannelId, ulong voiceChannelId, bool excluded, bool enabled, string prefix)
        {
            Id = id;
            GuildId = guildId;
            TextChannelId = textChannelId;
            VoiceChannelId = voiceChannelId;
            Excluded = excluded;
            Enabled = enabled;
            Prefix = prefix;
        }
    }
}