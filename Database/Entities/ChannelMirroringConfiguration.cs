using Database.Entities.Base;

namespace Database.Entities
{
    public class ChannelMirroringConfiguration : GuildChannelEntity
    {
        public ulong DestinationChannelId { get; set; }
        public ulong WebhookId { get; set; }

        public ChannelMirroringConfiguration(ulong guildId, ulong channelId) : base(guildId, channelId) { }
    }
}
