using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Users
    {
        public static async Task<List<UserRow>> GetRowsAsync(ulong? userId = null)
        {
            List<UserRow> matchedRows = new List<UserRow>();

            string command = "SELECT * FROM Users WHERE TRUE";
            List<(string, object)> values = new List<(string, object)>();

            if (userId.HasValue)
            {
                command += " AND UserId = @UserId";
                values.Add(("UserId", userId.Value));
            }

            MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

            while (reader.Read())
            {
                matchedRows.Add(UserRow.FromDatabase(
                    reader.GetUInt64(0),
                    reader.GetString(1),
                    reader.GetDateTime(2),
                    reader.GetInt32(3)));
            }

            reader.Close();

            return matchedRows;
        }

        public static async Task<UserRow> GetRowAsync(ulong userId)
        {
            List<UserRow> rows = await GetRowsAsync(userId);
            return rows.Count > 0 ? rows.First() : new UserRow(userId);
        }

        public static async Task SaveRowAsync(UserRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO Users (UserId, Email, LastVisit, Visits) VALUES (@UserId, @Email, @LastVisit, @Visits);",
                    ("UserId", row.UserId),
                    ("Email", row.Email), 
                    ("LastVisit", row.LastVisit),
                    ("Visits", row.Visits));

                row.New = false;
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE Users SET Email = @Email, LastVisit = @LastVisit WHERE UserId = @UserId;",
                    ("UserId", row.UserId),
                    ("Email", row.Email), 
                    ("LastVisit", row.LastVisit));
            }
        }

        public static async Task AddNewVisitAsync(ulong userId)
        {
            await Sql.ExecuteAsync("UPDATE Users SET Visits = Visits + 1 WHERE UserId = @UserId;",
                ("UserId", userId));
        }
    }

    public class UserRow
    {
        public bool New { get; set; }
        public ulong UserId { get; set; }
        public string Email { get; set; }
        public DateTime LastVisit { get; set; }
        public int Visits { get; set; }

        private UserRow()
        {
        }

        public UserRow(ulong userId)
        {
            New = true;
            UserId = userId;
            LastVisit = DateTime.MinValue;
        }

        public static UserRow FromDatabase(ulong userId, string email, DateTime lastVisit, int visits)
        {
            return new UserRow
            {
                New = false,
                UserId = userId,
                Email = email,
                LastVisit = lastVisit,
                Visits = visits
            };
        }
    }
}
