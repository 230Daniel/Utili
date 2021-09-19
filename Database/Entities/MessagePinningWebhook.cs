using Database.Entities.Base;

namespace Database.Entities
{
    public class MessagePinningWebhook : GuildChannelEntity
    {
        public ulong WebhookId { get; set; }

        public MessagePinningWebhook(ulong guildId, ulong channelId) : base(guildId, channelId) { }
    }
}
