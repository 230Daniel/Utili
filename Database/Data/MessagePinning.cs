using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class MessagePinning
    {
        public static List<MessagePinningRow> GetRows(ulong? guildId = null, bool ignoreCache = false)
        {
            List<MessagePinningRow> matchedRows = new List<MessagePinningRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.MessagePinning.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM MessagePinning WHERE TRUE";
                List<(string, string)> values = new List<(string, string)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(MessagePinningRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetUInt64(1),
                        reader.GetUInt64(2),
                        reader.GetBoolean(3)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static MessagePinningRow GetRow(ulong guildId)
        {
            List<MessagePinningRow> rows = GetRows(guildId);
            return rows.Count > 0 ? rows.First() : new MessagePinningRow(guildId);
        }

        public static void SaveRow(MessagePinningRow row)
        {
            MySqlCommand command;

            if (row.New)
            {
                command = Sql.GetCommand($"INSERT INTO MessagePinning (GuildId, PinChannelId, WebhookId, Pin) VALUES (@GuildId, @PinChannelId, @WebhookId, {Sql.ToSqlBool(row.Pin)});",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("PinChannelId", row.PinChannelId.ToString()),
                        ("WebhookId", row.WebhookId.ToString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;

                if(Cache.Initialised) Cache.MessagePinning.Rows.Add(row);
            }
            else
            {
                command = Sql.GetCommand($"UPDATE MessagePinning SET PinChannelId = @PinChannelId, WebhookId = @WebhookId, Pin = {Sql.ToSqlBool(row.Pin)} WHERE GuildId = @GuildId;",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("PinChannelId", row.PinChannelId.ToString()),
                        ("WebhookId", row.WebhookId.ToString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.MessagePinning.Rows[Cache.MessagePinning.Rows.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static void DeleteRow(MessagePinningRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.MessagePinning.Rows.RemoveAll(x => x.GuildId == row.GuildId);

            string commandText = "DELETE FROM MessagePinning WHERE GuildId = @GuildId";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {
                ("GuildId", row.GuildId.ToString())});

            command.ExecuteNonQuery();
            command.Connection.Close();
        }
    }

    public class MessagePinningTable
    {
        public List<MessagePinningRow> Rows { get; set; }
    }

    public class MessagePinningRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public ulong PinChannelId { get; set; }
        public ulong WebhookId { get; set; }
        public bool Pin { get; set; }

        private MessagePinningRow()
        {
        }

        public MessagePinningRow(ulong guildId)
        {
            New = true;
            GuildId = guildId;
            PinChannelId = 0;
            Pin = false;
        }

        public static MessagePinningRow FromDatabase(ulong guildId, ulong pinChannelId, ulong webhookId, bool pin)
        {
            return new MessagePinningRow
            {
                New = false,
                GuildId = guildId,
                PinChannelId = pinChannelId,
                WebhookId = webhookId,
                Pin = pin
            };
        }
    }
}
