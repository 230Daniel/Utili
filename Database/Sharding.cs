using System;
using MySql.Data.MySqlClient;
using static Database.Sql;

namespace Database
{
    public class Sharding
    {
        public static int GetTotalShards()
        {
            MySqlDataReader reader = GetCommand("SELECT * FROM Sharding WHERE Id = 1;").ExecuteReader();

            reader.Read();
            int result = reader.GetInt32(1);
            reader.Close();

            return result;
        }

        public static void UpdateShardStats(int shards, int lowerShardId, int guilds)
        {
            MySqlCommand command = GetCommand(
                "UPDATE Sharding SET Heartbeat = @Heartbeat, Guilds = @Guilds WHERE Shards = @Shards AND LowerShardId = @LowerShardId",
                new[]
                {
                    ("Heartbeat", ToSqlDateTime(DateTime.UtcNow)),
                    ("Guilds", guilds.ToString()),
                    ("Shards", shards.ToString()),
                    ("LowerShardId", lowerShardId.ToString())
                });

            int modifiedRows = command.ExecuteNonQuery();
            command.Connection.Close();

            if (modifiedRows == 0)
            {
                command = GetCommand(
                    "INSERT INTO Sharding(Shards, LowerShardId, Heartbeat, Guilds) VALUES(@Shards, @LowerShardId, @Heartbeat, @Guilds)",
                    new[]
                    {
                        ("Heartbeat", ToSqlDateTime(DateTime.UtcNow)),
                        ("Guilds", guilds.ToString()),
                        ("Shards", shards.ToString()),
                        ("LowerShardId", lowerShardId.ToString())
                    });

                command.ExecuteNonQuery();
                command.Connection.Close();
            }
        }

        public static int GetGuildCount()
        {
            MySqlDataReader reader = GetCommand("SELECT Guilds FROM Sharding WHERE Heartbeat > @MinimumHeartbeat AND Guilds IS NOT NULL",
                new[]
                {
                    ("MinimumHeartbeat", ToSqlDateTime(DateTime.UtcNow - TimeSpan.FromSeconds(30)))
                }).ExecuteReader();

            int guilds = 0;
            while (reader.Read())
            {
                guilds += reader.GetInt64(0);
            }

            reader.Close();

            return guilds;
        }
    }
}
