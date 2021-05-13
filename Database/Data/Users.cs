using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Users
    {
        
        public static async Task<List<UserRow>> GetRowsAsync(ulong? userId = null, string customerId = null)
        {
            List<UserRow> matchedRows = new();

            string command = "SELECT * FROM Users WHERE TRUE";
            List<(string, object)> values = new();

            if (userId.HasValue)
            {
                command += " AND UserId = @UserId";
                values.Add(("UserId", userId.Value));
            }

            if (!string.IsNullOrEmpty(customerId))
            {
                command += " AND CustomerId = @CustomerId";
                values.Add(("CustomerId", customerId));
            }

            MySqlDataReader reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

            while (reader.Read())
            {
                matchedRows.Add(UserRow.FromDatabase(
                    reader.GetUInt64(0),
                    reader.GetString(1),
                    reader.GetDateTime(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3)));
            }

            reader.Close();

            return matchedRows;
        }

        public static async Task<UserRow> GetRowAsync(ulong userId)
        {
            List<UserRow> rows = await GetRowsAsync(userId);
            return rows.Count > 0 ? rows.First() : new UserRow(userId);
        }

        public static async Task<UserRow> GetRowAsync(string customerId)
        {
            List<UserRow> rows = await GetRowsAsync(customerId: customerId);
            return rows.Count > 0 ? rows.First() : null;
        }

        public static async Task SaveRowAsync(UserRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync("INSERT INTO Users (UserId, Email, LastVisit, CustomerId) VALUES (@UserId, @Email, @LastVisit, @CustomerId);",
                    ("UserId", row.UserId),
                    ("Email", row.Email), 
                    ("LastVisit", row.LastVisit),
                    ("CustomerId", row.CustomerId));

                row.New = false;
            }
            else
            {
                await Sql.ExecuteAsync("UPDATE Users SET Email = @Email, LastVisit = @LastVisit, CustomerId = @CustomerId WHERE UserId = @UserId;",
                    ("UserId", row.UserId.ToString()),
                    ("Email", row.Email), 
                    ("LastVisit", row.LastVisit),
                    ("CustomerId", row.CustomerId));
            }
        }

        public static async Task DeleteRowAsync(UserRow row)
        {
            await Sql.ExecuteAsync(
                "DELETE FROM Users WHERE UserId = @UserId",
                ("UserId", row.UserId));
        }
    }

    public class UserRow : IRow
    {
        public bool New { get; set; }
        public ulong UserId { get; set; }
        public string Email { get; set; }
        public DateTime LastVisit { get; set; }
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

        public static UserRow FromDatabase(ulong userId, string email, DateTime lastVisit, string customerId)
        {
            return new()
            {
                New = false,
                UserId = userId,
                Email = email,
                LastVisit = lastVisit,
                CustomerId = customerId
            };
        }

        public async Task SaveAsync()
        {
            await Users.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await Users.DeleteRowAsync(this);
        }
    }
}
