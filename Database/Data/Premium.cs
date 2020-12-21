using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Premium
    {
        public static List<PremiumRow> GetRows(ulong? userId = null, ulong? guildId = null, bool ignoreCache = false)
        {
            List<PremiumRow> matchedRows = new List<PremiumRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Premium.Rows);

                if (userId.HasValue) matchedRows.RemoveAll(x => x.UserId != userId.Value);
                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM Premium WHERE TRUE";
                List<(string, string)> values = new List<(string, string)>();

                if (userId.HasValue)
                {
                    command += " AND UserId = @UserId";
                    values.Add(("UserId", userId.Value.ToString()));
                }

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(PremiumRow.FromDatabase(
                        reader.GetInt32(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static List<PremiumRow> GetUserRows(ulong userId)
        {
            List<PremiumRow> rows = GetRows(userId).OrderBy(x => x.SlotId).ToList();
            int amount = Subscriptions.GetSlotCount(userId);

            rows = rows.Take(amount).ToList();
            while (rows.Count < amount)
            {
                PremiumRow row = new PremiumRow(userId, 0);
                SaveRow(row);
                rows.Add(row);
            }

            return rows;
        }

        public static void SaveRow(PremiumRow row)
        {
            MySqlCommand command;

            if (row.New)
            {
                command = Sql.GetCommand("INSERT INTO Premium (UserId, GuildId) VALUES (@UserId, @GuildId);",
                    new [] { ("UserId", row.UserId.ToString()),
                        ("GuildId", row.GuildId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;
                row.SlotId = GetRows(row.UserId, row.GuildId).First().SlotId;

                if(Cache.Initialised) Cache.Premium.Rows.Add(row);
            }
            else
            {
                command = Sql.GetCommand("UPDATE Premium SET GuildId = @GuildId, UserId = @UserId WHERE SlotId = @SlotId",
                    new [] { ("UserId", row.UserId.ToString()),
                        ("GuildId", row.GuildId.ToString()),
                        ("SlotId", row.SlotId.ToString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.Premium.Rows[Cache.Premium.Rows.FindIndex(x => x.SlotId == row.SlotId)] = row;
            }
        }

        public static void DeleteRow(PremiumRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.Premium.Rows.RemoveAll(x => x.SlotId == row.SlotId);

            string commandText = "DELETE FROM Premium WHERE SlotId = @SlotId;";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {
                ("SlotId", row.SlotId.ToString())});

            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class PremiumTable
    {
        public List<PremiumRow> Rows { get; set; }
    }

    public class PremiumRow
    {
        public bool New { get; set; }
        public int SlotId { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }

        private PremiumRow()
        {

        }

        public PremiumRow(ulong userId, ulong guildId)
        {
            New = true;
            UserId = userId;
            GuildId = guildId;
        }

        public static PremiumRow FromDatabase(int slotId, ulong userId, ulong guildId)
        {
            return new PremiumRow
            {
                New = false,
                SlotId = slotId,
                UserId = userId,
                GuildId = guildId
            };
        }
    }
}
