using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Mozilla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Database.Data
{
    public class VoiceRoles
    {
        public static List<VoiceRolesRow> GetRows(ulong? guildId = null, ulong? channelId = null, bool ignoreCache = false)
        {
            List<VoiceRolesRow> matchedRows = new List<VoiceRolesRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.VoiceRoles.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
            }
            else
            {
                string command = "SELECT * FROM VoiceRoles WHERE TRUE";
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
                    matchedRows.Add(new VoiceRolesRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static void SaveRow(VoiceRolesRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand($"INSERT INTO VoiceRoles (GuildID, ChannelId, RoleId) VALUES (@GuildId, @ChannelId, @RoleId);",
                    new [] { ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("RoleId", row.RoleId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();
                row.Id = GetRows(row.GuildId, row.ChannelId, true).First().Id;
                
                if(Cache.Initialised) Cache.VoiceRoles.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand($"UPDATE VoiceRoles SET GuildId = @GuildId, ChannelId = @ChannelId, RoleId = @RoleId WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("RoleId", row.RoleId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.VoiceRoles.Rows[Cache.VoiceRoles.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void DeleteRow(VoiceRolesRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.VoiceRoles.Rows.RemoveAll(x => x.Id == row.Id);

            string commandText = "DELETE FROM VoiceRoles WHERE Id = @Id";
            MySqlCommand command = Sql.GetCommand(commandText, new[] {("Id", row.Id.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class VoiceRolesTable
    {
        public List<VoiceRolesRow> Rows { get; set; }

        public void Load()
        // Load the table from the database
        {
            List<VoiceRolesRow> newRows = new List<VoiceRolesRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM VoiceRoles;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new VoiceRolesRow(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3)));
                }
            }
            catch {}

            reader.Close();
            Rows = newRows;
        }
    }

    public class VoiceRolesRow
    {
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong RoleId { get; set; }

        public VoiceRolesRow()
        {
            Id = 0;
        }

        public VoiceRolesRow(int id, ulong guildId, ulong channelId, ulong roleId)
        {
            Id = id;
            GuildId = guildId;
            ChannelId = channelId;
            RoleId = roleId;
        }
    }
}