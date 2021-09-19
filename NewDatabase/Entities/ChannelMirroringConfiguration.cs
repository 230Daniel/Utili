using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class ChannelMirroringConfiguration : GuildChannelEntity
    {
        public ulong DestinationChannelId { get; set; }
        public ulong WebhookId { get; set; }

        public ChannelMirroringConfiguration(ulong guildId, ulong channelId) : base(guildId, channelId) { }
    }
}
