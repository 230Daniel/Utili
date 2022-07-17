namespace Utili.Database.Entities.Base;

public class MemberEntity
{
    public ulong GuildId { get; internal set; }
    public ulong MemberId { get; internal set; }

    protected MemberEntity(ulong guildId, ulong memberId)
    {
        GuildId = guildId;
        MemberId = memberId;
    }

    internal MemberEntity() { }
}