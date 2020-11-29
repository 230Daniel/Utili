using System.Linq;
using System.Timers;
using static Utili.Program;

namespace Utili
{
    internal static class Sharding
    {
        public static void Update(object sender, ElapsedEventArgs e)
        {
            Database.Sharding.UpdateShardStats(_client.Shards.Count, _client.Shards.OrderBy(x => x.ShardId).First().ShardId, _client.Guilds.Count);
        }
    }
}
