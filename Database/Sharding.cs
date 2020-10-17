using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using static Database.Sql;

namespace Database
{
    public class Sharding
    {
        public static int GetTotalShards()
        {
            MySqlDataReader reader = Sql.GetCommand("SELECT * FROM Sharding WHERE Id = 1;").ExecuteReader();

            reader.Read();

            return reader.GetInt32(1);
        }

        public static void UpdateShardStats(int shards, int lowerShardId, int guilds)
        {
            int modifiedRows = GetCommand(
                "UPDATE Sharding SET Heartbeat = @Heartbeat, Guilds = @Guilds WHERE Shards = @Shards AND LowerShardId = @LowerShardId",
                new[]
                {
                    ("Heartbeat", ConvertToSqlTime(DateTime.UtcNow)),
                    ("Guilds", guilds.ToString()),
                    ("Shards", shards.ToString()),
                    ("LowerShardId", lowerShardId.ToString())
                }).ExecuteNonQuery();

            if (modifiedRows == 0)
            {
                GetCommand(
                    "INSERT INTO Sharding(Shards, LowerShardId, Heartbeat, Guilds) VALUES(@Shards, @LowerShardId, @Heartbeat, @Guilds)",
                    new[]
                    {
                        ("Heartbeat", ConvertToSqlTime(DateTime.UtcNow)),
                        ("Guilds", guilds.ToString()),
                        ("Shards", shards.ToString()),
                        ("LowerShardId", lowerShardId.ToString())
                    }).ExecuteNonQuery();
            }
        }

        public static int GetGuildCount()
        {
            MySqlDataReader reader = GetCommand("SELECT Guilds FROM Sharding WHERE Heartbeat > @MinimumHeartbeat AND Guilds IS NOT NULL",
                new[]
                {
                    ("MinimumHeartbeat", ConvertToSqlTime(DateTime.UtcNow - TimeSpan.FromSeconds(30)))
                }).ExecuteReader();

            int guilds = 0;
            while (reader.Read())
            {
                guilds += reader.GetInt32(0);
            }

            return guilds;
        }
    }
}
