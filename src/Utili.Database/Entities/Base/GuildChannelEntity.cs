namespace Utili.Database.Entities.Base;

public class GuildChannelEntity
{
    public ulong GuildId { get; internal set; }
    public ulong ChannelId { get; internal set; }

    protected GuildChannelEntity(ulong guildId, ulong channelId)
    {
        GuildId = guildId;
        ChannelId = channelId;
    }

    internal GuildChannelEntity() { }
}