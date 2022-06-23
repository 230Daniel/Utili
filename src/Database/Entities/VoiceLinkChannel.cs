using Database.Entities.Base;

namespace Database.Entities
{
    public class VoiceLinkChannel : GuildChannelEntity
    {
        public ulong TextChannelId { get; set; }

        public VoiceLinkChannel(ulong guildId, ulong channelId) : base(guildId, channelId) { }
    }
}
