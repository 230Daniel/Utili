namespace Utili.Backend.Models;

public class MessageLogsBulkDeletedMessagesModel
{
    public string Timestamp { get; init; }
    public int MessagesDeleted { get; init; }
    public int MessagesLogged { get; init; }
    public MessageLogsBulkDeletedMessageModel[] Messages { get; init; }
}
