using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Database
{
    public static class Sharding
    {
        public static async Task<int> GetTotalShardsAsync()
        {
            MySqlDataReader reader = await Sql.ExecuteReaderAsync("SELECT * FROM Sharding WHERE Id = 1;");

            reader.Read();
            int result = reader.GetInt32(1);
            reader.Close();

            return result;
        }

        public static async Task UpdateShardStatsAsync(int shards, int lowerShardId, int guilds)
        {
            int affected = await Sql.ExecuteAsync(
                "UPDATE Sharding SET Heartbeat = @Heartbeat, Guilds = @Guilds WHERE Shards = @Shards AND LowerShardId = @LowerShardId",
                ("Heartbeat", DateTime.UtcNow),
                ("Guilds", guilds),
                ("Shards", shards),
                ("LowerShardId", lowerShardId));

            if (affected == 0)
            {
                await Sql.ExecuteAsync(
                    "INSERT INTO Sharding(Shards, LowerShardId, Heartbeat, Guilds) VALUES(@Shards, @LowerShardId, @Heartbeat, @Guilds)",
                    ("Heartbeat", DateTime.UtcNow),
                    ("Guilds", guilds),
                    ("Shards", shards),
                    ("LowerShardId", lowerShardId));
            }
        }

        public static async Task<int> GetGuildCountAsync()
        {
            MySqlDataReader reader = await Sql.ExecuteReaderAsync(
                "SELECT SUM(Guilds) FROM Sharding WHERE Heartbeat > @MinimumHeartbeat AND Guilds IS NOT NULL",
                ("MinimumHeartbeat", DateTime.UtcNow - TimeSpan.FromSeconds(30)));

            reader.Read();
            int guilds = reader.GetInt32(0);
            reader.Close();

            return guilds;
        }
    }
}
