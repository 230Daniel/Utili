using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class ChannelMirroring
    {
        public static List<ChannelMirroringRow> GetRows(ulong? guildId = null, ulong? fromChannelId = null, bool ignoreCache = false)
        {
            List<ChannelMirroringRow> matchedRows = new List<ChannelMirroringRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.ChannelMirroring.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (fromChannelId.HasValue) matchedRows.RemoveAll(x => x.FromChannelId != fromChannelId.Value);
            }
            else
            {
                string command = "SELECT * FROM ChannelMirroring WHERE TRUE";
                List<(string, string)> values = new List<(string, string)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                if (fromChannelId.HasValue)
                {
                    command += " AND FromChannelId = @FromChannelId";
                    values.Add(("FromChannelId", fromChannelId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(ChannelMirroringRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static void SaveRow(ChannelMirroringRow row)
        {
            MySqlCommand command;

            if (row.New) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO ChannelMirroring (GuildId, FromChannelId, ToChannelId, WebhookId) VALUES (@GuildId, @FromChannelId, @ToChannelId, @WebhookId);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("FromChannelId", row.FromChannelId.ToString()),
                        ("ToChannelId", row.ToChannelId.ToString()),
                        ("WebhookId", row.WebhookId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;

                if(Cache.Initialised) Cache.ChannelMirroring.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE ChannelMirroring SET ToChannelId = @ToChannelId, WebhookId = @WebhookId WHERE GuildId = @GuildId AND FromChannelId = @FromChannelId;",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("FromChannelId", row.FromChannelId.ToString()),
                        ("ToChannelId", row.ToChannelId.ToString()),
                        ("WebhookId", row.WebhookId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.ChannelMirroring.Rows[Cache.ChannelMirroring.Rows.FindIndex(x => x.GuildId == row.GuildId && x.FromChannelId == row.FromChannelId)] = row;
            }
        }

        public static void SaveWebhookId(ChannelMirroringRow row)
        {
            MySqlCommand command;

            if (row.New) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO ChannelMirroring (GuildId, FromChannelId, ToChannelId, WebhookId) VALUES (@GuildId, @FromChannelId, @ToChannelId, @WebhookId);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("FromChannelId", row.FromChannelId.ToString()),
                        ("ToChannelId", row.ToChannelId.ToString()),
                        ("WebhookId", row.WebhookId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;

                if(Cache.Initialised) Cache.ChannelMirroring.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE ChannelMirroring SET WebhookId = @WebhookId WHERE GuildId = @GuildId AND FromChannelId = @FromChannelId;",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("FromChannelId", row.FromChannelId.ToString()),
                        ("WebhookId", row.WebhookId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.ChannelMirroring.Rows[Cache.ChannelMirroring.Rows.FindIndex(x => x.GuildId == row.GuildId && x.FromChannelId == row.FromChannelId)] = row;
            }
        }

        public static void DeleteRow(ChannelMirroringRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.ChannelMirroring.Rows.RemoveAll(x => x.GuildId == row.GuildId && x.FromChannelId == row.FromChannelId);

            string commandText = "DELETE FROM ChannelMirroring WHERE GuildId = @GuildId AND FromChannelId = @FromChannelId";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {
                    ("GuildId", row.GuildId.ToString()),
                    ("FromChannelId", row.FromChannelId.ToString())});

            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class ChannelMirroringTable
    {
        public List<ChannelMirroringRow> Rows { get; set; }
    }

    public class ChannelMirroringRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong FromChannelId { get; set; }
        public ulong ToChannelId { get; set; }
        public ulong WebhookId { get; set; }

        private ChannelMirroringRow()
        {

        }

        public ChannelMirroringRow(ulong guildId, ulong fromChannelId)
        {
            New = true;
            GuildId = guildId;
            FromChannelId = fromChannelId;
        }

        public static ChannelMirroringRow FromDatabase(ulong guildId, ulong fromChannelId, ulong toChannelId, ulong webhookId)
        {
            return new ChannelMirroringRow
            {
                New = false,
                GuildId = guildId,
                FromChannelId = fromChannelId,
                ToChannelId = toChannelId,
                WebhookId = webhookId
            };
        }
    }
}
