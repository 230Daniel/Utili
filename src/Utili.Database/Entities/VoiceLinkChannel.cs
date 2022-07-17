using Utili.Database.Entities.Base;

namespace Utili.Database.Entities;

public class VoiceLinkChannel : GuildChannelEntity
{
    public ulong TextChannelId { get; set; }

    public VoiceLinkChannel(ulong guildId, ulong channelId) : base(guildId, channelId) { }
}