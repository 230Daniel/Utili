using System;

namespace Utili.Database.Entities
{
    public class ShardDetail
    {
        public int ShardId { get; internal set; }
        public int Guilds { get; set; }
        public DateTime Heartbeat { get; set; }

        public ShardDetail(int shardId)
        {
            ShardId = shardId;
        }

        internal ShardDetail() { }
    }
}
