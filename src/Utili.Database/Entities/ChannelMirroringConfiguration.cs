using Utili.Database.Entities.Base;

namespace Utili.Database.Entities;

public class ChannelMirroringConfiguration : GuildChannelEntity
{
    public ulong DestinationChannelId { get; set; }
    public ChannelMirroringAuthorDisplayMode AuthorDisplayMode { get; set; }

    public ChannelMirroringConfiguration(ulong guildId, ulong channelId) : base(guildId, channelId) { }
}

public enum ChannelMirroringAuthorDisplayMode
{
    WebhookName,
    MessageContent
}