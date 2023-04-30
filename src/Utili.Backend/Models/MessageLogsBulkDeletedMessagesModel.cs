namespace Utili.Backend.Models;

public class MessageLogsBulkDeletedMessagesModel
{
    public string Timestamp { get; init; }
    public int MessagesDeleted { get; init; }
    public int MessagesRecorded { get; init; }
    public string[] Messages { get; init; }
}
