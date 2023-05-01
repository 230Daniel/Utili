using System;
using System.Collections.Generic;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities;

public class MessageLogsBulkDeletedMessages : GuidEntity
{
    public DateTime Timestamp { get; set; }
    public int MessagesDeleted { get; set; }
    public int MessagesLogged { get; set; }
    public List<MessageLogsBulkDeletedMessage> Messages { get; set; }
}
