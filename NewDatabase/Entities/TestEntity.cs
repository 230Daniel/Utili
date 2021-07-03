using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class TestEntity : GuildChannelEntity
    {
        public string Value { get; set; }

        public TestEntity(ulong guildId, ulong channelId) : base(guildId, channelId) { }
    }
}
