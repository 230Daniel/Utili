    using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Data
{
    public static class RoleLinking
    {
        public static async Task<List<RoleLinkingRow>> GetRowsAsync(ulong? guildId = null, ulong? linkId = null, bool ignoreCache = false)
        {
            var matchedRows = new List<RoleLinkingRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.RoleLinking);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (linkId.HasValue) matchedRows.RemoveAll(x => x.LinkId != linkId.Value);
            }
            else
            {
                var command = "SELECT * FROM RoleLinking WHERE TRUE";
                var values = new List<(string, object)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                if (linkId.HasValue)
                {
                    command += " AND LinkId = @LinkId";
                    values.Add(("LinkId", linkId.Value));
                }

                var reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(RoleLinkingRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3),
                        reader.GetInt32(4)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<RoleLinkingRow> GetRowAsync(ulong guildId, ulong linkId)
        {
            var rows = await GetRowsAsync(guildId, linkId);
            return rows.Count > 0 ? rows.First() : new RoleLinkingRow(guildId, 0, 0);
        }

        public static async Task SaveRowAsync(RoleLinkingRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync("INSERT INTO RoleLinking (GuildId, RoleId, LinkedRoleId, Mode) VALUES (@GuildId, @RoleId, @LinkedRoleId, @Mode);",
                    ("GuildId", row.GuildId), 
                    ("RoleId", row.RoleId),
                    ("LinkedRoleId", row.LinkedRoleId),
                    ("Mode", row.Mode));

                // LinkId is not set here so this row can not be edited until retrieved again
                row.New = false;
                if(Cache.Initialised) Cache.RoleLinking.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE RoleLinking SET LinkedRoleId = @LinkedRoleId, Mode = @Mode WHERE LinkId = @LinkId;",
                    ("LinkId", row.LinkId),
                    ("LinkedRoleId", row.LinkedRoleId),
                    ("Mode", row.Mode));

                if(Cache.Initialised) Cache.RoleLinking[Cache.RoleLinking.FindIndex(x => x.GuildId == row.GuildId && x.RoleId == row.RoleId && x.LinkedRoleId == row.LinkedRoleId)] = row;
            }
        }

        public static async Task DeleteRowAsync(RoleLinkingRow row)
        {
            if(Cache.Initialised) Cache.RoleLinking.RemoveAll(x => x.GuildId == row.GuildId && x.RoleId == row.RoleId  && x.LinkedRoleId == row.LinkedRoleId);

            await Sql.ExecuteAsync(
                "DELETE FROM RoleLinking WHERE LinkId = @LinkId",
                ("LinkId", row.LinkId));
        }
    }

    public class RoleLinkingRow : IRow
    {
        public bool New { get; set; }
        public ulong LinkId { get; set; }
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public ulong LinkedRoleId { get; set; }
        public int Mode { get; set; }
        // 0 = When they get RoleId, add role LinkedRoleId
        // 1 = When they get RoleId, remove role LinkedRoleId
        // 2 = When they no longer have RoleId, add role LinkedRoleId
        // 3 = When they no longer have RoleId, remove role LinkedRoleId

        private RoleLinkingRow()
        {

        }

        public RoleLinkingRow(ulong guildId, ulong roleId, int mode)
        {
            New = true;
            LinkId = 0;
            GuildId = guildId;
            RoleId = roleId;
            LinkedRoleId = 0;
            Mode = mode;
        }

        public static RoleLinkingRow FromDatabase(ulong linkId, ulong guildId, ulong roleId, ulong linkedRoleId, int mode)
        {
            return new()
            {
                New = false,
                LinkId = linkId,
                GuildId = guildId,
                RoleId = roleId,
                LinkedRoleId = linkedRoleId,
                Mode = mode
            };
        }

        public async Task SaveAsync()
        {
            await RoleLinking.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await RoleLinking.DeleteRowAsync(this);
        }
    }
}
