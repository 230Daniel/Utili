using System;

namespace NewDatabase.Entities
{
    public class ShardDetail
    {
        public int ShardId { get; internal set; }
        public int Guilds { get; set; }
        public DateTime Heartbeat { get; set; }
    }
}
