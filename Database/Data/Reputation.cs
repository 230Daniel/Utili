using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Data
{
    public static class Reputation
    {
        public static async Task<List<ReputationRow>> GetRowsAsync(ulong? guildId = null, bool ignoreCache = false)
        {
            var matchedRows = new List<ReputationRow>();

            if (Cache.Initialised && !ignoreCache)
            {
                matchedRows.AddRange(Cache.Reputation);

                if (guildId.HasValue) matchedRows.RemoveAll(x => x.GuildId != guildId.Value);
            }
            else
            {
                var command = "SELECT * FROM Reputation WHERE TRUE";
                
                var values = new List<(string, object)>();

                if (guildId.HasValue)
                {
                    command += " AND GuildId = @GuildId";
                    values.Add(("GuildId", guildId.Value));
                }

                var reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

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

        public static async Task<ReputationRow> GetRowAsync(ulong guildId)
        {
            var rows = await GetRowsAsync(guildId);
            return rows.Count > 0 ? rows.First() : new ReputationRow(guildId);
        }

        public static async Task SaveRowAsync(ReputationRow row)
        {
            if (row.New)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO Reputation (GuildId, Emotes) VALUES (@GuildId, @Emotes);",
                    ("GuildId", row.GuildId), 
                    ("Emotes", row.GetEmotesString()));

                row.New = false;
                if(Cache.Initialised) Cache.Reputation.Add(row);
            }
            else
            {
                await Sql.ExecuteAsync(
                    "UPDATE Reputation SET Emotes = @Emotes WHERE GuildId = @GuildId;",
                    ("GuildId", row.GuildId), 
                    ("Emotes", row.GetEmotesString()));

                if(Cache.Initialised) Cache.Reputation[Cache.Reputation.FindIndex(x => x.GuildId == row.GuildId)] = row;
            }
        }

        public static async Task DeleteRowAsync(ReputationRow row)
        {
            if(Cache.Initialised) Cache.Reputation.RemoveAll(x => x.GuildId == row.GuildId);

            await Sql.ExecuteAsync("DELETE FROM Reputation WHERE GuildId = @GuildId", 
                ("GuildId", row.GuildId));
        }

        public static async Task<List<ReputationUserRow>> GetUserRowsAsync(ulong? guildId = null, ulong? userId = null)
        {
            var matchedRows = new List<ReputationUserRow>();

            var command = "SELECT * FROM ReputationUsers WHERE TRUE";
            var values = new List<(string, object)>();

            if (guildId.HasValue)
            {
                command += " AND GuildId = @GuildId";
                values.Add(("GuildId", guildId.Value));
            }

            if (userId.HasValue)
            {
                command += " AND UserId = @UserId";
                values.Add(("UserId", userId.Value));
            }

            var reader = await Sql.ExecuteReaderAsync(command, values.ToArray());

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

        public static async Task<ReputationUserRow> GetUserRowAsync(ulong guildId, ulong userId)
        {
            var rows = await GetUserRowsAsync(guildId, userId);
            return rows.Count > 0 ? rows.First() : new ReputationUserRow(guildId, userId);
        }

        public static async Task AlterUserReputationAsync(ulong guildId, ulong userId, long reputationChange)
        {
            var affected = await Sql.ExecuteAsync("UPDATE ReputationUsers SET Reputation = Reputation + @ReputationChange WHERE GuildId = @GuildId AND UserId = @UserId;",
                ("GuildId", guildId), 
                ("UserId", userId),
                ("ReputationChange", reputationChange));

            if (affected == 0)
            {
                await Sql.ExecuteAsync("INSERT INTO ReputationUsers (GuildId, UserId, Reputation) VALUES(@GuildId, @UserId, @Reputation)",
                    ("GuildId", guildId), 
                    ("UserId", userId),
                    ("Reputation", reputationChange));
            }
        }

        public static async Task SetUserReputationAsync(ulong guildId, ulong userId, long reputation)
        {
            if (reputation == 0)
            {
                await Sql.ExecuteAsync("DELETE FROM ReputationUsers WHERE GuildId = @GuildId AND UserId = @UserId;",
                    ("GuildId", guildId), 
                    ("UserId", userId));
                return;
            }

            var affected = await Sql.ExecuteAsync("UPDATE ReputationUsers SET Reputation = @Reputation WHERE GuildId = @GuildId AND UserId = @UserId;",
                ("GuildId", guildId), 
                ("UserId", userId),
                ("Reputation", reputation));

            if (affected == 0)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO ReputationUsers (GuildId, UserId, Reputation) VALUES(@GuildId, @UserId, @Reputation)",
                    ("GuildId", guildId), 
                    ("UserId", userId),
                    ("Reputation", reputation));
            }
        }
    }
    public class ReputationRow : IRow
    {
        public bool New { get; set; }
        public ulong GuildId { get; set; }
        public List<(string, int)> Emotes { get; set; }

        private ReputationRow()
        {

        }

        public ReputationRow(ulong guildId)
        {
            New = true;
            GuildId = guildId;
            Emotes = new List<(string, int)>();
        }

        public static ReputationRow FromDatabase(ulong guildId, string emotes)
        {
            var row = new ReputationRow
            {
                New = false,
                GuildId = guildId,
                Emotes = new List<(string, int)>()
            };

            emotes = EString.FromEncoded(emotes).Value;
            if (!string.IsNullOrEmpty(emotes))
            {
                foreach (var emoteString in emotes.Split(","))
                {
                    if(string.IsNullOrWhiteSpace(emoteString))
                        continue;
                    
                    var value = int.Parse(emoteString.Split("///").Last());
                    var emote = emoteString.Split("///").First();
                    row.Emotes.Add((emote, value));
                }
            }

            return row;
        }

        public string GetEmotesString()
        {
            var emotesString = "";

            for (var i = 0; i < Emotes.Count; i++)
            {
                emotesString += $"{Emotes[i].Item1}///{Emotes[i].Item2}";
                if (i != Emotes.Count - 1)
                {
                    emotesString += ",";
                }
            }

            return EString.FromDecoded(emotesString).EncodedValue;
        }

        public async Task SaveAsync()
        {
            await Reputation.SaveRowAsync(this);
        }

        public async Task DeleteAsync()
        {
            await Reputation.DeleteRowAsync(this);
        }
    }

    public class ReputationUserRow : IRow
    {
        public bool New { get; set; } // Does nothing
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

        public async Task SaveAsync()
        {
            await Data.Reputation.SetUserReputationAsync(GuildId, UserId, Reputation);
        }

        public async Task DeleteAsync()
        {
            await Data.Reputation.SetUserReputationAsync(GuildId, UserId, 0);
            // Deletes row if rep = 0.
        }
    }
}
