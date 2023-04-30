namespace Utili.Database.Entities.Base;

public class GuildEntity
{
    public ulong GuildId { get; internal set; }

    protected GuildEntity(ulong guildId)
    {
        GuildId = guildId;
    }

    internal GuildEntity() { }
}
