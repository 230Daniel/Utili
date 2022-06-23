using System;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities
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
