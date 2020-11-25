using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public class ChannelMirroring
    {
        public static List<ChannelMirroringRow> GetRows(ulong? guildId = null, ulong? fromChannelId = null, long? id = null, bool ignoreCache = false)
        {
            List<ChannelMirroringRow> matchedRows = new List<ChannelMirroringRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.ChannelMirroring.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
                if (fromChannelId.HasValue) matchedRows.RemoveAll(x => x.FromChannelId != fromChannelId.Value);
                if (id.HasValue) matchedRows.RemoveAll(x => x.Id != id.Value);
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

                if (id.HasValue)
                {
                    command += " AND Id = @Id";
                    values.Add(("Id", id.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(new ChannelMirroringRow(
                        reader.GetInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3),
                        reader.GetUInt64(4)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static void SaveRow(ChannelMirroringRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO ChannelMirroring (GuildId, FromChannelId, ToChannelId, WebhookId) VALUES (@GuildId, @FromChannelId, @ToChannelId, @WebhookId);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("FromChannelId", row.FromChannelId.ToString()),
                        ("ToChannelId", row.ToChannelId.ToString()),
                        ("WebhookId", row.WebhookId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.Id = GetRows(row.GuildId, row.FromChannelId, ignoreCache: true).First().Id;

                if(Cache.Initialised) Cache.ChannelMirroring.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE ChannelMirroring SET GuildId = @GuildId, FromChannelId = @FromChannelId, ToChannelId = @ToChannelId, WebhookId = @WebhookId WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("GuildId", row.GuildId.ToString()), 
                        ("FromChannelId", row.FromChannelId.ToString()),
                        ("ToChannelId", row.ToChannelId.ToString()),
                        ("WebhookId", row.WebhookId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.ChannelMirroring.Rows[Cache.ChannelMirroring.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void SaveWebhookId(ChannelMirroringRow row)
        {
            MySqlCommand command;

            if (row.Id == 0) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO ChannelMirroring (GuildId, FromChannelId, ToChannelId, WebhookId) VALUES (@GuildId, @FromChannelId, @ToChannelId, @WebhookId);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("FromChannelId", row.FromChannelId.ToString()),
                        ("ToChannelId", row.ToChannelId.ToString()),
                        ("WebhookId", row.WebhookId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.Id = GetRows(row.GuildId, row.FromChannelId, ignoreCache: true).First().Id;

                if(Cache.Initialised) Cache.ChannelMirroring.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE ChannelMirroring SET WebhookId = @WebhookId WHERE Id = @Id;",
                    new [] {("Id", row.Id.ToString()),
                        ("WebhookId", row.WebhookId.ToString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.ChannelMirroring.Rows[Cache.ChannelMirroring.Rows.FindIndex(x => x.Id == row.Id)] = row;
            }
        }

        public static void DeleteRow(ChannelMirroringRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.ChannelMirroring.Rows.RemoveAll(x => x.Id == row.Id);

            string commandText = "DELETE FROM ChannelMirroring WHERE Id = @Id";
            MySqlCommand command = Sql.GetCommand(commandText, new[] {("Id", row.Id.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class ChannelMirroringTable
    {
        public List<ChannelMirroringRow> Rows { get; set; }

        public void Load()
        // Load the table from the database
        {
            List<ChannelMirroringRow> newRows = new List<ChannelMirroringRow>();

            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM ChannelMirroring;").ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    newRows.Add(new ChannelMirroringRow(
                        reader.GetInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetUInt64(3),
                        reader.GetUInt64(4)));
                }
            }
            catch {}

            reader.Close();

            Rows = newRows;
        }
    }

    public class ChannelMirroringRow
    {
        public long Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong FromChannelId { get; set; }
        public ulong ToChannelId { get; set; }
        public ulong WebhookId { get; set; }

        public ChannelMirroringRow()
        {
            Id = 0;
        }

        public ChannelMirroringRow(long id, ulong guildId, ulong fromChannelId, ulong toChannelId, ulong webhookId)
        {
            Id = id;
            GuildId = guildId;
            FromChannelId = fromChannelId;
            ToChannelId = toChannelId;
            WebhookId = webhookId;
        }
    }
}
