using System;
using Database.Entities.Base;

namespace Database.Entities
{
    public class MessageLogsMessage : MessageEntity
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong AuthorId { get; set; } 
        public DateTime Timestamp { get; set; }
        public string Content { get; set; }
        
        /// <summary>
        ///     If this message was sent in a thread, this will be the ID of its containing channel
        /// </summary>
        public ulong TextChannelId { get; set; }
        
        public MessageLogsMessage(ulong messageId) : base(messageId) { }
    }
}
