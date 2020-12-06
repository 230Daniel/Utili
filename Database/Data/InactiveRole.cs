using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class InactiveRole
    {
        private static readonly TimeSpan GapBetweenUpdates = TimeSpan.FromMinutes(60); 

        public static List<InactiveRoleRow> GetRows(ulong? guildId = null, bool ignoreCache = false)
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
                List<(string, string)> values = new List<(string, string)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

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

        public static List<InactiveRoleRow> GetUpdateRequiredRows(bool ignoreCache = false)
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
                List<(string, string)> values = new List<(string, string)>
                {
                    ("LastUpdate", Sql.ToSqlDateTime(DateTime.UtcNow - GapBetweenUpdates))
                };

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

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

        public static InactiveRoleRow GetRow(ulong guildId)
        {
            List<InactiveRoleRow> rows = GetRows(guildId);
            return rows.Count > 0 ? rows.First() : new InactiveRoleRow(guildId);
        }

        public static void SaveRow(InactiveRoleRow row)
        {
            MySqlCommand command;

            if (row.New) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand($"INSERT INTO InactiveRole (GuildId, RoleId, ImmuneRoleId, Threshold, Inverse, DefaultLastAction, LastUpdate) VALUES (@GuildId, @RoleId, @ImmuneRoleId, @Threshold, {Sql.ToSqlBool(row.Inverse)}, @DefaultLastAction, @LastUpdate);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("RoleId", row.RoleId.ToString()),
                        ("ImmuneRoleId", row.ImmuneRoleId.ToString()),
                        ("Threshold", row.Threshold.ToString()),
                        ("DefaultLastAction", Sql.ToSqlDateTime(row.DefaultLastAction)),
                        ("LastUpdate", Sql.ToSqlDateTime(row.LastUpdate))});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;

                if(Cache.Initialised) Cache.InactiveRole.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                // Not updating DefaultLastAction is intentional
                command = Sql.GetCommand($"UPDATE InactiveRole SET RoleId = @RoleId, ImmuneRoleId = @ImmuneRoleId, Threshold = @Threshold, Inverse = {Sql.ToSqlBool(row.Inverse)}, LastUpdate = @LastUpdate WHERE GuildId = @GuildId;",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("RoleId", row.RoleId.ToString()),
                        ("ImmuneRoleId", row.ImmuneRoleId.ToString()),
                        ("Threshold", row.Threshold.ToString()),
                        ("LastUpdate", Sql.ToSqlDateTime(row.LastUpdate))});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.InactiveRole.Rows[Cache.InactiveRole.Rows.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static void SaveLastUpdate(InactiveRoleRow row)
        {
            MySqlCommand command;

            if (row.New) 
            // The row is a new entry so should be inserted into the database
            {
                // If the if statement is true then something has gone horribly wrong, but it will work anyway.
                command = Sql.GetCommand($"INSERT INTO InactiveRole (GuildId, RoleId, ImmuneRoleId, Threshold, Inverse, DefaultLastAction, LastUpdate) VALUES (@GuildId, @RoleId, @ImmuneRoleId, @Threshold, {Sql.ToSqlBool(row.Inverse)}, @DefaultLastAction, @LastUpdate);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("RoleId", row.RoleId.ToString()),
                        ("ImmuneRoleId", row.ImmuneRoleId.ToString()),
                        ("Threshold", row.Threshold.ToString()),
                        ("DefaultLastAction", Sql.ToSqlDateTime(row.DefaultLastAction)),
                        ("LastUpdate", Sql.ToSqlDateTime(row.LastUpdate))});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;

                if(Cache.Initialised) Cache.InactiveRole.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE InactiveRole SET LastUpdate = @LastUpdate WHERE GuildId = @GuildId;",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("LastUpdate", Sql.ToSqlDateTime(row.LastUpdate))});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.InactiveRole.Rows[Cache.InactiveRole.Rows.FindIndex(x => x.GuildId == row.GuildId)].LastUpdate = row.LastUpdate;
            }
        }

        public static void DeleteRow(InactiveRoleRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.InactiveRole.Rows.RemoveAll(x => x.GuildId == row.GuildId);

            string commandText = "DELETE FROM InactiveRole WHERE GuildId = @GuildId";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {("GuildId", row.GuildId.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }

        public static void UpdateUser(ulong guildId, ulong userId)
        {
            MySqlCommand command = Sql.GetCommand("UPDATE InactiveRoleUsers SET LastAction = @LastAction WHERE GuildId = @GuildId AND UserId = @UserId",
                new [] {("GuildId", guildId.ToString()), 
                    ("UserId", userId.ToString()),
                    ("LastAction", Sql.ToSqlDateTime(DateTime.UtcNow))});

            if (command.ExecuteNonQuery() == 0)
            {
                command = Sql.GetCommand("INSERT INTO InactiveRoleUsers (GuildId, UserId, LastAction) VALUES (@GuildId, @UserId, @LastAction)",
                    new [] {("GuildId", guildId.ToString()), 
                        ("UserId", userId.ToString()),
                        ("LastAction", Sql.ToSqlDateTime(DateTime.UtcNow))});

                command.ExecuteNonQuery();
            }
            
            command.Connection.Close();
        }

        public static List<InactiveRoleUserRow> GetUsers(ulong guildId)
        {
            List<InactiveRoleUserRow> matchedRows = new List<InactiveRoleUserRow>();

            string command = "SELECT * FROM InactiveRoleUsers WHERE GuildId = @GuildId";
            List<(string, string)> values = new List<(string, string)>
            {
                ("GuildId", guildId.ToString())
            };

            MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

            while (reader.Read())
            {
                matchedRows.Add(new InactiveRoleUserRow(
                    reader.GetUInt64(0),
                    reader.GetUInt64(0),
                    reader.GetDateTime(1)));
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
