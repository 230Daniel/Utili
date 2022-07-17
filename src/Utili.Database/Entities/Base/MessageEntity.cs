namespace Utili.Database.Entities.Base;

public class MessageEntity
{
    public ulong MessageId { get; internal set; }

    protected MessageEntity(ulong messageId)
    {
        MessageId = messageId;
    }

    internal MessageEntity() { }
}