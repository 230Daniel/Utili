using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public class Subscriptions
    {
        public static List<SubscriptionsRow> GetRows(string subscriptionId = null, ulong? userId = null)
        {
            List<SubscriptionsRow> matchedRows = new List<SubscriptionsRow>();

            string command = "SELECT * FROM Subscriptions WHERE TRUE";
            List<(string, string)> values = new List<(string, string)>();

            if (!string.IsNullOrEmpty(subscriptionId))
            {
                command += " AND SubscriptionId = @SubscriptionId";
                values.Add(("SubscriptionId", subscriptionId));
            }

            if (userId.HasValue)
            {
                command += " AND UserId = @UserId";
                values.Add(("UserId", userId.Value.ToString()));
            }

            MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

            while (reader.Read())
            {
                matchedRows.Add(SubscriptionsRow.FromDatabase(
                    reader.GetString(0),
                    reader.GetUInt64(1),
                    reader.GetDateTime(2),
                    reader.GetInt32(3)));
            }

            reader.Close();

            return matchedRows;
        }

        public static SubscriptionsRow GetRow(string subscriptionId)
        {
            List<SubscriptionsRow> rows = GetRows(subscriptionId);
            return rows.Count > 0 ? rows.First() : new SubscriptionsRow(subscriptionId);
        }

        public static void SaveRow(SubscriptionsRow row)
        {
            MySqlCommand command;

            if (row.New)
            {
                command = Sql.GetCommand("INSERT INTO Subscriptions (SubscriptionId, UserId, EndsAt, Slots) VALUES (@SubscriptionId, @UserId, @EndsAt, @Slots);",
                    new []
                    {
                        ("SubscriptionId", row.SubscriptionId),
                        ("UserId", row.UserId.ToString()),
                        ("EndsAt", Sql.ToSqlDateTime(row.EndsAt)),
                        ("Slots", row.Slots.ToString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;
            }
            else
            {
                command = Sql.GetCommand(
                    "UPDATE Subscriptions SET UserId = @UserId, EndsAt = @EndsAt, Slots = @Slots WHERE SubscriptionId = @SubscriptionId;",
                    new[]
                    {
                        ("SubscriptionId", row.SubscriptionId),
                        ("UserId", row.UserId.ToString()),
                        ("EndsAt", Sql.ToSqlDateTime(row.EndsAt)),
                        ("Slots", row.Slots.ToString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
            }
        }

        public static void DeleteRow(SubscriptionsRow row)
        {
            if(row == null) return;

            string commandText = "DELETE FROM Subscriptions WHERE SubscriptionId = @SubscriptionId;";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {
                    ("SubscriptionId", row.SubscriptionId)
                });

            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class SubscriptionsRow
    {
        public bool New { get; set; }
        public string SubscriptionId { get; set; }
        public DateTime EndsAt { get; set; }
        public ulong UserId { get; set; }
        public int Slots { get; set; }

        private SubscriptionsRow()
        {

        }

        public SubscriptionsRow(string subscriptionId)
        {
            New = true;
            SubscriptionId = subscriptionId;
        }

        public static SubscriptionsRow FromDatabase(string subscriptionId, ulong userId, DateTime endsAt, int slots)
        {
            return new SubscriptionsRow
            {
                New = false,
                SubscriptionId = subscriptionId,
                UserId = userId,
                EndsAt = endsAt,
                Slots = slots
            };
        }
    }
}
