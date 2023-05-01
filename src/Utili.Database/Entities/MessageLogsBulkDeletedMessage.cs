using System;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities;

public class MessageLogsBulkDeletedMessage : MessageEntity
{
    public string Username { get; set; }
    public DateTime Timestamp { get; set; }
    public string Content { get; set; }

    public MessageLogsBulkDeletedMessage(ulong messageId) : base(messageId) { }
}
