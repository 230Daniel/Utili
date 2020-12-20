using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public class Users
    {
        public static List<UserRow> GetRows(ulong? userId = null)
        {
            List<UserRow> matchedRows = new List<UserRow>();

            string command = "SELECT * FROM Users WHERE TRUE";
            List<(string, string)> values = new List<(string, string)>();

            if (userId.HasValue)
            {
                command += " AND UserId = @UserId";
                values.Add(("UserId", userId.Value.ToString()));
            }

            MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

            while (reader.Read())
            {
                matchedRows.Add(UserRow.FromDatabase(
                    reader.GetUInt64(0),
                    reader.GetString(1),
                    reader.GetDateTime(2),
                    reader.GetInt32(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4)));
            }

            reader.Close();

            return matchedRows;
        }

        public static UserRow GetRow(ulong userId)
        {
            List<UserRow> rows = GetRows(userId);
            return rows.Count > 0 ? rows.First() : new UserRow(userId);
        }

        public static void SaveRow(UserRow row)
        {
            MySqlCommand command;

            if (row.New)
            {
                command = Sql.GetCommand("INSERT INTO Users (UserId, Email, LastVisit, Visits, CustomerId) VALUES (@UserId, @Email, @LastVisit, @Visits, @CustomerId);",
                    new [] {
                        ("UserId", row.UserId.ToString()),
                        ("Email", row.Email), 
                        ("LastVisit", Sql.ToSqlDateTime(row.LastVisit)),
                        ("Visits", row.Visits.ToString()),
                        ("CustomerId", row.CustomerId)
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;
            }
            else
            {
                command = Sql.GetCommand("UPDATE Users SET Email = @Email, LastVisit = @LastVisit, CustomerId = @CustomerId WHERE UserId = @UserId;",
                    new [] {
                        ("UserId", row.UserId.ToString()),
                        ("Email", row.Email), 
                        ("LastVisit", Sql.ToSqlDateTime(row.LastVisit)),
                        ("CustomerId", row.CustomerId)
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
            }
        }

        public static void AddNewVisit(ulong userId)
        {
            MySqlCommand command = Sql.GetCommand("UPDATE Users SET Visits = Visits + 1 WHERE UserId = @UserId;",
                new [] {
                    ("UserId", userId.ToString())});

            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class UserRow
    {
        public bool New { get; set; }
        public ulong UserId { get; set; }
        public string Email { get; set; }
        public DateTime LastVisit { get; set; }
        public int Visits { get; set; }
        public string CustomerId { get; set; }

        private UserRow()
        {
        }

        public UserRow(ulong userId)
        {
            New = true;
            UserId = userId;
            LastVisit = DateTime.MinValue;
            CustomerId = null;
        }

        public static UserRow FromDatabase(ulong userId, string email, DateTime lastVisit, int visits, string customerId)
        {
            return new UserRow
            {
                New = false,
                UserId = userId,
                Email = email,
                LastVisit = lastVisit,
                Visits = visits,
                CustomerId = customerId
            };
        }
    }
}
