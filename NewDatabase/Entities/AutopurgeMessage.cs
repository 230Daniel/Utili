using System;
using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class AutopurgeMessage : MessageEntity
    {
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public DateTime Timestamp { get; init; }
        public bool IsBot { get; init; }
        public bool IsPinned { get; set; }

        public AutopurgeMessage(ulong messageId) : base(messageId) { }
    }
}
