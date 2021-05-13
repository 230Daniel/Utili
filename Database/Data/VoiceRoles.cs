using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class VoiceRoles
    {
        public static async Task<List<VoiceRolesRow>> GetRowsAsync(ulong? guildId = null, ulong? channelId = null, bool ignoreCache = false)
        {
            List<VoiceRolesRow> matchedRows = new();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.VoiceRoles);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (channelId.HasValue) matchedRows.RemoveAll(x => x.ChannelId != channelId.Value);
            }
            else
            {
                string command = "SELECT * FROM VoiceRoles WHERE TRUE";
                List<(string, object)> values = new();

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
                    matchedRows.Add(VoiceRolesRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<VoiceRolesRow> GetRowAsync(ulong guildId, ulong channelId)
        {
            var rows = await GetRowsAsync(guildId, channelId);
            return rows.Count > 0 ? rows.First() : new VoiceRolesRow(guildId, channelId);
        }
        public static async Task SaveRowAsync(VoiceRolesRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO VoiceRoles (GuildId, ChannelId, RoleId) VALUES (@GuildId, @ChannelId, @RoleId);",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("RoleId", row.RoleId));

                row.New = false;
                if(Cache.Initialised) Cache.VoiceRoles.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE VoiceRoles SET RoleId = @RoleId WHERE GuildId = @GuildId AND ChannelId = @ChannelId;",
                    ("GuildId", row.GuildId), 
                    ("ChannelId", row.ChannelId),
                    ("RoleId", row.RoleId));

                if(Cache.Initialised) Cache.VoiceRoles[Cache.VoiceRoles.FindIndex(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId)] = row;
            }
        }

        public static async Task DeleteRowAsync(VoiceRolesRow row)
        {
            if(Cache.Initialised) Cache.VoiceRoles.RemoveAll(x => x.GuildId == row.GuildId && x.ChannelId == row.ChannelId);

            await Sql.ExecuteAsync(
                "DELETE FROM VoiceRoles WHERE GuildId = @GuildId AND ChannelId = @ChannelId",
                ("GuildId", row.GuildId),
                ("ChannelId", row.ChannelId));
        }
    }
    public class VoiceRolesRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong RoleId { get; set; }

        private VoiceRolesRow()
        {
            
        }

        public VoiceRolesRow(ulong guildId, ulong channelId)
        {
            New = true;
            GuildId = guildId;
            ChannelId = channelId;
        }

        public static VoiceRolesRow FromDatabase(ulong guildId, ulong channelId, ulong roleId)
        {
            return new()
            {
                New = false,
                GuildId = guildId,
                ChannelId = channelId,
                RoleId = roleId
            };
        }

        public async Task SaveAsync()
        {
            await VoiceRoles.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await VoiceRoles.DeleteRowAsync(this);
        }
    }
}