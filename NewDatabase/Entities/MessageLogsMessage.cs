using System;
using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class MessageLogsMessage : MessageEntity
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong AuthorId { get; set; } 
        public DateTime Timestamp { get; set; }
        public string Content { get; set; }
        
        public MessageLogsMessage(ulong messageId) : base(messageId) { }
    }
}
