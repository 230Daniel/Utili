using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public class InactiveRole
    {
        private static readonly TimeSpan GapBetweenUpdates = TimeSpan.FromMinutes(60); 

        public static List<InactiveRoleRow> GetRows(ulong? guildId = null, long? id = null, bool ignoreCache = false)
        {
            List<InactiveRoleRow> matchedRows = new List<InactiveRoleRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.InactiveRole.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (id.HasValue) matchedRows.RemoveAll(x => x.Id != id.Value);
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

                if (id.HasValue)
                {
                    command += " AND Id = @Id";
                    values.Add(("Id", id.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(new InactiveRoleRow(
                        reader.GetInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3),
                        reader.GetString(4),
                        reader.GetBoolean(5),
                        reader.GetDateTime(6),
                        reader.GetDateTime(7)));
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
                    matchedRows.Add(new InactiveRoleRow(
                        reader.GetInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3),
                        reader.GetString(4),
                        reader.GetBoolean(5),
                        reader.GetDateTime(6),
                        reader.GetDateTime(7)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static void SaveRow(InactiveRoleRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
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

                row.Id = GetRows(row.GuildId, ignoreCache: true).First().Id;

                if(Cache.Initialised) Cache.InactiveRole.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                // Not updating DefaultLastAction is intentional
                command = Sql.GetCommand($"UPDATE InactiveRole SET GuildId = @GuildId, RoleId = @RoleId, ImmuneRoleId = @ImmuneRoleId, Threshold = @Threshold, Inverse = {Sql.ToSqlBool(row.Inverse)}, LastUpdate = @LastUpdate WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("RoleId", row.RoleId.ToString()),
                        ("ImmuneRoleId", row.ImmuneRoleId.ToString()),
                        ("Threshold", row.Threshold.ToString()),
                        ("LastUpdate", Sql.ToSqlDateTime(row.LastUpdate))});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.InactiveRole.Rows[Cache.InactiveRole.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void SaveLastUpdate(InactiveRoleRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
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

                row.Id = GetRows(row.GuildId, ignoreCache: true).First().Id;

                if(Cache.Initialised) Cache.InactiveRole.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE InactiveRole SET LastUpdate = @LastUpdate WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("LastUpdate", Sql.ToSqlDateTime(row.LastUpdate))});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.InactiveRole.Rows[Cache.InactiveRole.Rows.FindIndex(x => x.Id == row.Id)].LastUpdate = row.LastUpdate;
            }
        }

        public static void DeleteRow(InactiveRoleRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.InactiveRole.Rows.RemoveAll(x => x.Id == row.Id);

            string commandText = "DELETE FROM InactiveRole WHERE Id = @Id";
            MySqlCommand command = Sql.GetCommand(commandText, new[] {("Id", row.Id.ToString())});
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

        public void Load()
        // Load the table from the database
        {
            List<InactiveRoleRow> newRows = new List<InactiveRoleRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM InactiveRole;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new InactiveRoleRow(
                        reader.GetInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3),
                        reader.GetString(4),
                        reader.GetBoolean(5),
                        reader.GetDateTime(6),
                        reader.GetDateTime(7)));
                }
            }
            catch {}

            reader.Close();

            Rows = newRows;
        }
    }

    public class InactiveRoleRow
    {
        public long Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong RoleId { get; set; }
        public ulong ImmuneRoleId { get; set; }
        public TimeSpan Threshold { get; set; }
        public bool Inverse { get; set; }
        public DateTime DefaultLastAction { get; set; }
        public DateTime LastUpdate { get; set; }
        
        public InactiveRoleRow()
        {
            Id = 0;
        }

        public InactiveRoleRow(long id, ulong guildId, ulong roleId, ulong immuneRoleId, string threshold, bool inverse, DateTime defaultLastAction, DateTime lastUpdate)
        {
            Id = id;
            GuildId = guildId;
            RoleId = roleId;
            ImmuneRoleId = immuneRoleId;
            Threshold = TimeSpan.Parse(threshold);
            Inverse = inverse;
            DefaultLastAction = defaultLastAction;
            LastUpdate = lastUpdate;
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
