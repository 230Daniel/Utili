﻿using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Roles
    {
        public static List<RolesRow> GetRows(ulong? guildId = null, long? id = null, bool ignoreCache = false)
        {
            List<RolesRow> matchedRows = new List<RolesRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Roles.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (id.HasValue) matchedRows.RemoveAll(x => x.Id != id.Value);
            }
            else
            {
                string command = "SELECT * FROM Roles WHERE TRUE";
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
                    matchedRows.Add(new RolesRow(
                        reader.GetInt64(0),
                        reader.GetUInt64(1),
                        reader.GetBoolean(2),
                        reader.GetString(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static RolesRow GetRow(ulong guildId)
        {
            List<RolesRow> rows = GetRows(guildId);
            return rows.Count > 0 ? rows.First() : new RolesRow(guildId);
        }

        public static void SaveRow(RolesRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand($"INSERT INTO Roles (GuildId, RolePersist, JoinRoles) VALUES (@GuildId, {Sql.ToSqlBool(row.RolePersist)}, @JoinRoles);",
                    new [] {
                        ("GuildId", row.GuildId.ToString()),
                        ("JoinRoles", row.GetJoinRolesString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.Id = GetRows(row.GuildId, ignoreCache: true).First().Id;

                if(Cache.Initialised) Cache.Roles.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand($"UPDATE Roles SET GuildId = @GuildId, RolePersist = {Sql.ToSqlBool(row.RolePersist)}, JoinRoles = @JoinRoles WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("JoinRoles", row.GetJoinRolesString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.Roles.Rows[Cache.Roles.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void DeleteRow(RolesRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.Roles.Rows.RemoveAll(x => x.Id == row.Id);

            string commandText = "DELETE FROM Roles WHERE Id = @Id";
            MySqlCommand command = Sql.GetCommand(commandText, new[] {("Id", row.Id.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }


        public static List<RolesPersistantRolesRow> GetPersistRows(ulong? guildId = null, ulong? userId = null, long? id = null)
        {
            List<RolesPersistantRolesRow> matchedRows = new List<RolesPersistantRolesRow>();

            string command = "SELECT * FROM RolesPersistantRoles WHERE TRUE";
            List<(string, string)> values = new List<(string, string)>();

            if (guildId.HasValue)
            {
                command += " AND GuildId = @GuildId";
                values.Add(("GuildId", guildId.Value.ToString()));
            }

            if (userId.HasValue)
            {
                command += " AND UserId = @UserId";
                values.Add(("UserId", userId.Value.ToString()));
            }

            if (id.HasValue)
            {
                command += " AND Id = @Id";
                values.Add(("Id", id.Value.ToString()));
            }

            MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

            while (reader.Read())
            {
                matchedRows.Add(new RolesPersistantRolesRow(
                    reader.GetInt64(0),
                    reader.GetUInt64(1),
                    reader.GetString(3)));
            }

            reader.Close();

            return matchedRows;
        }

        public static void SavePersistRow(RolesPersistantRolesRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO RolesPersistantRoles (GuildId, UserId, Roles) VALUES (@GuildId, @UserId, @Roles);",
                    new [] {
                        ("GuildId", row.GuildId.ToString()),
                        ("UserId", row.UserId.ToString()),
                        ("Roles", row.GetRolesString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.Id = GetPersistRows(row.GuildId, row.UserId).First().Id;
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE RolesPersistantRoles SET GuildId = @GuildId, UserId = @UserId, Roles = @Roles WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("UserId", row.UserId.ToString()),
                        ("Roles", row.GetRolesString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
            }
        }

        public static void DeletePersistRow(RolesPersistantRolesRow row)
        {
            if(row == null) return;

            string commandText = "DELETE FROM RolesPersistantRoles WHERE Id = @Id";
            MySqlCommand command = Sql.GetCommand(commandText, new[] {("Id", row.Id.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class RolesTable
    {
        public List<RolesRow> Rows { get; set; }

        public void Load()
        // Load the table from the database
        {
            List<RolesRow> newRows = new List<RolesRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM Roles;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new RolesRow(
                        reader.GetInt64(0),
                        reader.GetUInt64(1),
                        reader.GetBoolean(2),
                        reader.GetString(3)));
                }
            }
            catch {}

            reader.Close();

            Rows = newRows;
        }
    }

    public class RolesRow
    {
        public long Id { get; set; }
        public ulong GuildId { get; set; }
        public bool RolePersist { get; set; }
        public List<ulong> JoinRoles { get; set; }

        public RolesRow(ulong guildId)
        {
            Id = 0;
            GuildId = guildId;
            RolePersist = false;
            JoinRoles = new List<ulong>();
        }

        public RolesRow(long id, ulong guildId, bool rolePersist, string joinRoles)
        {
            Id = id;
            GuildId = guildId;
            RolePersist = rolePersist;

            JoinRoles = new List<ulong>();

            if (!string.IsNullOrEmpty(joinRoles))
            {
                foreach (string joinRole in joinRoles.Split(","))
                {
                    if (ulong.TryParse(joinRole, out ulong channelId))
                    {
                        JoinRoles.Add(channelId);
                    }
                }
            }
        }

        public string GetJoinRolesString()
        {
            string joinRolesString = "";

            for (int i = 0; i < JoinRoles.Count; i++)
            {
                ulong joinRole = JoinRoles[i];
                joinRolesString += joinRole.ToString();
                if (i != JoinRoles.Count - 1)
                {
                    joinRolesString += ",";
                }
            }

            return joinRolesString;
        }
    }

    public class RolesPersistantRolesRow
    {
        public long Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public List<ulong> Roles { get; set; }

        public RolesPersistantRolesRow()
        {
            Id = 0;
        }

        public RolesPersistantRolesRow(long id, ulong guildId, string roles)
        {
            Id = id;
            GuildId = guildId;

            Roles = new List<ulong>();

            if (!string.IsNullOrEmpty(roles))
            {
                foreach (string role in roles.Split(","))
                {
                    if (ulong.TryParse(role, out ulong channelId))
                    {
                        Roles.Add(channelId);
                    }
                }
            }
        }

        public string GetRolesString()
        {
            string rolesString = "";

            for (int i = 0; i < Roles.Count; i++)
            {
                ulong role = Roles[i];
                rolesString += role.ToString();
                if (i != Roles.Count - 1)
                {
                    rolesString += ",";
                }
            }

            return rolesString;
        }
    }
}