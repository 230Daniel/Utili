using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class VoiceRoles
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
                    matchedRows.Add(VoiceRolesRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static void SaveRow(VoiceRolesRow row)
        {
            MySqlCommand command;

            if (row.New)
            {
                command = Sql.GetCommand("INSERT INTO VoiceRoles (GuildId, ChannelId, RoleId) VALUES (@GuildId, @ChannelId, @RoleId);",
                    new [] { ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("RoleId", row.RoleId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();
                row.New = false;
                
                if(Cache.Initialised) Cache.VoiceRoles.Rows.Add(row);
            }
            else
            {
                command = Sql.GetCommand("UPDATE VoiceRoles SET RoleId = @RoleId WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("ChannelId", row.ChannelId.ToString()),
                        ("RoleId", row.RoleId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.VoiceRoles.Rows[Cache.VoiceRoles.Rows.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static void DeleteRow(VoiceRolesRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.VoiceRoles.Rows.RemoveAll(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId);

            string commandText = "DELETE FROM VoiceRoles WHERE GuildId = @GuildId AND ChannelId = @ChannelId";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {
                    ("GuildId", row.GuildId.ToString()),
                    ("ChannelId", row.ChannelId.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class VoiceRolesTable
    {
        public List<VoiceRolesRow> Rows { get; set; }
    }

    public class VoiceRolesRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong RoleId { get; set; }

        public VoiceRolesRow()
        {
            New = true;
        }

        public static VoiceRolesRow FromDatabase(ulong guildId, ulong channelId, ulong roleId)
        {
            return new VoiceRolesRow
            {
                New = false,
                GuildId = guildId,
                ChannelId = channelId,
                RoleId = roleId
            };
        }
    }
}