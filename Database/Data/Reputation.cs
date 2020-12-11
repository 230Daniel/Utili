using System.Collections.Generic;
using System.Linq;
using Discord;
using MySql.Data.MySqlClient;

namespace Database.Data
{
    public static class Reputation
    {
        public static List<ReputationRow> GetRows(ulong? guildId = null, bool ignoreCache = false)
        {
            List<ReputationRow> matchedRows = new List<ReputationRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Reputation.Rows);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                string command = "SELECT * FROM Reputation WHERE TRUE";
                List<(string, string)> values = new List<(string, string)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value.ToString()));
                }

                MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

                while (reader.Read())
                {
                    matchedRows.Add(ReputationRow.FromDatabase(
                        reader.GetUInt64(0),
                        reader.GetString(1)));
                }

                reader.Close();
            }

            return matchedRows;
        }

        public static ReputationRow GetRow(ulong guildId)
        {
            List<ReputationRow> rows = GetRows(guildId);
            return rows.Count > 0 ? rows.First() : new ReputationRow(guildId);
        }

        public static void SaveRow(ReputationRow row)
        {
            MySqlCommand command;

            if (row.New) 
            // The row is a new entry so should be inserted into the database
            {
                command = Sql.GetCommand("INSERT INTO Reputation (GuildId, Emotes) VALUES (@GuildId, @Emotes);",
                    new [] {("GuildId", row.GuildId.ToString()), 
                        ("Emotes", row.GetEmotesString())});

                command.ExecuteNonQuery();
                command.Connection.Close();

                row.New = false;

                if(Cache.Initialised) Cache.Reputation.Rows.Add(row);
            }
            else
            // The row already exists and should be updated
            {
                command = Sql.GetCommand("UPDATE Reputation SET Emotes = @Emotes WHERE GuildId = @GuildId;",
                    new [] {
                        ("GuildId", row.GuildId.ToString()), 
                        ("Emotes", row.GetEmotesString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();

                if(Cache.Initialised) Cache.Reputation.Rows[Cache.Reputation.Rows.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static void DeleteRow(ReputationRow row)
        {
            if(row == null) return;

            if(Cache.Initialised) Cache.Reputation.Rows.RemoveAll(x => x.GuildId == row.GuildId);

            string commandText = "DELETE FROM Reputation WHERE GuildId = @GuildId";
            MySqlCommand command = Sql.GetCommand(commandText, 
                new[] {("GuildId", row.GuildId.ToString())});
            command.ExecuteNonQuery();
            command.Connection.Close();
        }

        public static List<ReputationUserRow> GetUserRows(ulong? guildId = null, ulong? userId = null)
        {
            List<ReputationUserRow> matchedRows = new List<ReputationUserRow>();

            string command = "SELECT * FROM ReputationUsers WHERE TRUE";
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

            MySqlDataReader reader = Sql.GetCommand(command, values.ToArray()).ExecuteReader();

            while (reader.Read())
            {
                matchedRows.Add(new ReputationUserRow(
                    reader.GetUInt64(0),
                    reader.GetUInt64(1),
                    reader.GetInt64(2)));
            }

            reader.Close();

            return matchedRows;
        }

        public static ReputationUserRow GetUserRow(ulong guildId, ulong userId)
        {
            List<ReputationUserRow> rows = GetUserRows(guildId, userId);
            return rows.Count > 0 ? rows.First() : new ReputationUserRow(guildId, userId);
        }

        public static void AlterUserReputation(ulong guildId, ulong userId, long reputationChange)
        {
            MySqlCommand command = Sql.GetCommand("UPDATE ReputationUsers SET Reputation = Reputation + @ReputationChange WHERE GuildId = @GuildId AND UserId = @UserId;",
                new [] {("GuildId", guildId.ToString()), 
                    ("UserId", userId.ToString()),
                    ("ReputationChange", reputationChange.ToString())});

            if (command.ExecuteNonQuery() == 0)
            {
                command = Sql.GetCommand("INSERT INTO ReputationUsers (GuildId, UserId, Reputation) VALUES(@GuildId, @UserId, @Reputation)",
                    new [] {("GuildId", guildId.ToString()), 
                        ("UserId", userId.ToString()),
                        ("Reputation", reputationChange.ToString())});

                command.ExecuteNonQuery();
            }

            command.Connection.Close();
        }

        public static void SetUserReputation(ulong guildId, ulong userId, long reputation)
        {
            MySqlCommand command = Sql.GetCommand("UPDATE ReputationUsers SET Reputation = @Reputation WHERE GuildId = @GuildId AND UserId = @UserId;",
                new [] {("GuildId", guildId.ToString()), 
                    ("UserId", userId.ToString()),
                    ("Reputation", reputation.ToString())});

            if (command.ExecuteNonQuery() == 0)
            {
                command = Sql.GetCommand("INSERT INTO ReputationUsers (GuildId, UserId, Reputation) VALUES(@GuildId, @UserId, @Reputation)",
                    new [] {("GuildId", guildId.ToString()), 
                        ("UserId", userId.ToString()),
                        ("Reputation", reputation.ToString())});

                command.ExecuteNonQuery();
            }

            command.Connection.Close();
        }
    }

    public class ReputationTable
    {
        public List<ReputationRow> Rows { get; set; }
    }

    public class ReputationRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public List<(IEmote, int)> Emotes { get; set; }

        private ReputationRow()
        {

        }

        public ReputationRow(ulong guildId)
        {
            New = true;
            GuildId = guildId;
            Emotes = new List<(IEmote, int)>();
        }

        public static ReputationRow FromDatabase(ulong guildId, string emotes)
        {
            ReputationRow row = new ReputationRow
            {
                New = false,
                GuildId = guildId,
                Emotes = new List<(IEmote, int)>()
            };

            emotes = EString.FromEncoded(emotes).Value;
            if (!string.IsNullOrEmpty(emotes))
            {
                foreach (string emoteString in emotes.Split(","))
                {
                    int value = int.Parse(emoteString.Split("///").Last());
                    if (Emote.TryParse(emoteString.Split("///").First(), out Emote emote))
                    {
                        row.Emotes.Add((emote, value));
                    }
                    else
                    {
                        row.Emotes.Add((new Emoji(emoteString), value));
                    }
                }
            }

            return row;
        }

        public string GetEmotesString()
        {
            string emotesString = "";

            for (int i = 0; i < Emotes.Count; i++)
            {
                emotesString += $"{Emotes[i].Item1}///{Emotes[i].Item2}";
                if (i != Emotes.Count - 1)
                {
                    emotesString += ",";
                }
            }

            return EString.FromDecoded(emotesString).EncodedValue;
        }
    }

    public class ReputationUserRow
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public long Reputation { get; set; }

        public ReputationUserRow(ulong guildId, ulong userId)
        {
            GuildId = guildId;
            UserId = userId;
            Reputation = 0;
        }

        public ReputationUserRow(ulong guildId, ulong userId, long reputation)
        {
            GuildId = guildId;
            UserId = userId;
            Reputation = reputation;
        }
    }
}
