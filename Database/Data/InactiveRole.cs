﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class InactiveRole
    {
        private static readonly TimeSpan GapBetweenUpdates = TimeSpan.FromMinutes(60); 

        public static async Task<List<InactiveRoleRow>> GetRowsAsync(ulong? guildId = null, bool ignoreCache = false)
        {
            List<InactiveRoleRow> matchedRows = new List<InactiveRoleRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.InactiveRole.Rows);
                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM InactiveRole WHERE TRUE";
                List<(string, object)> values = new List<(string, object)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(InactiveRoleRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetString(3),
                        reader.GetBoolean(4),
                        reader.GetDateTime(5),
                        reader.GetDateTime(6)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<List<InactiveRoleRow>> GetUpdateRequiredRowsAsync(bool ignoreCache = false)
        {
            List<InactiveRoleRow> matchedRows = new List<InactiveRoleRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.InactiveRole.Rows);
                matchedRows.RemoveAll(x => DateTime.UtcNow - x.LastUpdate < GapBetweenUpdates);
            }
            else
            {
                string command = "SELECT * FROM InactiveRole WHERE LastUpdate < @LastUpdate";
                List<(string, object)> values = new List<(string, object)>
                {
                    ("LastUpdate", DateTime.UtcNow - GapBetweenUpdates)
                };

                MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

                while (reader.Read())
                {
                    matchedRows.Add(InactiveRoleRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetString(3),
                        reader.GetBoolean(4),
                        reader.GetDateTime(5),
                        reader.GetDateTime(6)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static async Task<InactiveRoleRow> GetRowAsync(ulong guildId)
        {
            List<InactiveRoleRow> rows = await GetRowsAsync(guildId);
            return rows.Count > 0 ? rows.First() : new InactiveRoleRow(guildId);
        }

        public static async Task SaveRowAsync(InactiveRoleRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO InactiveRole (GuildId, RoleId, ImmuneRoleId, Threshold, Inverse, DefaultLastAction, LastUpdate) VALUES (@GuildId, @RoleId, @ImmuneRoleId, @Threshold, @Inverse, @DefaultLastAction, @LastUpdate);",
                    ("GuildId", row.GuildId), 
                    ("RoleId", row.RoleId),
                    ("ImmuneRoleId", row.ImmuneRoleId),
                    ("Threshold", row.Threshold),
                    ("Inverse", row.Inverse),
                    ("DefaultLastAction", DateTime.UtcNow),
                    ("LastUpdate", DateTime.UtcNow - TimeSpan.FromMinutes(5)));

                row.New = false;
                if(Cache.Initialised) Cache.InactiveRole.Rows.Add(row);
            }
            else
            {
                // Not updating DefaultLastAction is intentional
                // TODO: If they're setting it from no role to a role we should reset DefaultLastAction to UTC Now.
                // Alternatively, always record activity data once a role has been set even if it's not there still.

                await Sql.ExecuteAsync(
                    "UPDATE InactiveRole SET RoleId = @RoleId, ImmuneRoleId = @ImmuneRoleId, Threshold = @Threshold, Inverse = @Inverse, LastUpdate = @LastUpdate WHERE GuildId = @GuildId;",
                    ("GuildId", row.GuildId), 
                    ("RoleId", row.RoleId),
                    ("ImmuneRoleId", row.ImmuneRoleId),
                    ("Threshold", row.Threshold),
                    ("Inverse", row.Inverse),
                    ("LastUpdate", row.LastUpdate));

                if(Cache.Initialised) Cache.InactiveRole.Rows[Cache.InactiveRole.Rows.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static async Task SaveLastUpdateAsync(InactiveRoleRow row)
        {
            if (row.New)
            {
                // Something has gone horifically wrong if a row doesn't exist for a server being updated
                await SaveRowAsync(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE InactiveRole SET LastUpdate = @LastUpdate WHERE GuildId = @GuildId;",
                    ("GuildId", row.GuildId), 
                    ("LastUpdate", row.LastUpdate));

                if(Cache.Initialised) Cache.InactiveRole.Rows[Cache.InactiveRole.Rows.FindIndex(x => x.GuildId == row.GuildId)].LastUpdate = row.LastUpdate;
            }
        }

        public static async Task DeleteRowAsync(InactiveRoleRow row)
        {
            if(Cache.Initialised) Cache.InactiveRole.Rows.RemoveAll(x => x.GuildId == row.GuildId);

            await Sql.ExecuteAsync(
                "DELETE FROM InactiveRole WHERE GuildId = @GuildId", 
                ("GuildId", row.GuildId));
        }

        public static async Task UpdateUserAsync(ulong guildId, ulong userId, DateTime? lastAction = null)
        {
            if (lastAction == null) lastAction = DateTime.UtcNow;

            int affected = await Sql.ExecuteAsync(
                "UPDATE InactiveRoleUsers SET LastAction = @LastAction WHERE GuildId = @GuildId AND UserId = @UserId",
                ("GuildId", guildId), 
                ("UserId", userId),
                ("LastAction", lastAction.Value));

            if (affected == 0)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO InactiveRoleUsers (GuildId, UserId, LastAction) VALUES (@GuildId, @UserId, @LastAction)",
                    ("GuildId", guildId), 
                    ("UserId", userId),
                    ("LastAction", lastAction.Value));
            }
        }

        public static async Task<List<InactiveRoleUserRow>> GetUsersAsync(ulong guildId)
        {
            List<InactiveRoleUserRow> matchedRows = new List<InactiveRoleUserRow>();

            string command = "SELECT * FROM InactiveRoleUsers WHERE GuildId = @GuildId";
            List<(string, object)> values = new List<(string, object)>
            {
                ("GuildId", guildId)
            };

            MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

            while (reader.Read())
            {
                matchedRows.Add(new InactiveRoleUserRow(
                    reader.GetUInt64(0),
                    reader.GetUInt64(1),
                    reader.GetDateTime(2)));
            }

            reader.Close();
            return matchedRows;
        }
    }

    public class InactiveRoleTable
    {
        public List<InactiveRoleRow> Rows { get; set; }
    }

    public class InactiveRoleRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public ulong ImmuneRoleId { get; set; }
        public TimeSpan Threshold { get; set; }
        public bool Inverse { get; set; }
        public DateTime DefaultLastAction { get; set; }
        public DateTime LastUpdate { get; set; }

        private InactiveRoleRow()
        {

        }

        public InactiveRoleRow(ulong guildId)
        {
            New = true;
            GuildId = guildId;
            RoleId = 0;
            ImmuneRoleId = 0;
            Threshold = TimeSpan.FromDays(30);
            Inverse = false;
            DefaultLastAction = DateTime.MinValue;
            LastUpdate = DateTime.MinValue;
        }

        public static InactiveRoleRow FromDatabase(ulong guildId, ulong roleId, ulong immuneRoleId, string threshold, bool inverse, DateTime defaultLastAction, DateTime lastUpdate)
        {
            return new InactiveRoleRow
            {
                New = false,
                GuildId = guildId,
                RoleId = roleId,
                ImmuneRoleId = immuneRoleId,
                Threshold = TimeSpan.Parse(threshold),
                Inverse = inverse,
                DefaultLastAction = defaultLastAction,
                LastUpdate = lastUpdate
            };
        }
    }

    public class InactiveRoleUserRow
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTime LastAction { get; set; }

        public InactiveRoleUserRow(ulong guildId, ulong userId, DateTime lastAction)
        {
            GuildId = guildId;
            UserId = userId;
            LastAction = lastAction;
        }
    }
}
