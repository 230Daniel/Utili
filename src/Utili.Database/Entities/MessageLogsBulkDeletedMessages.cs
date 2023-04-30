using System;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities;

public class MessageLogsBulkDeletedMessages : GuidEntity
{
    public DateTime Timestamp { get; set; }
    public int MessagesDeleted { get; set; }
    public int MessagesLogged { get; set; }
    public string[] Messages { get; set; }
}
