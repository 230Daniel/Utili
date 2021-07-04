using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class JoinMessageConfiguration : GuildEntity
    {
        public bool Enabled { get; set; }
        public JoinMessageMode Mode { get; set; }
        public ulong ChannelId { get; set; }
        public string Title { get; set; }
        public string Footer { get; set; }
        public string Content { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
        public string Thumbnail { get; set; }
        public string Icon { get; set; }
        public uint Colour { get; set; }

        public JoinMessageConfiguration(ulong guildId) : base(guildId) { }
    }

    public enum JoinMessageMode
    {
        Channel,
        DirectMessage
    }
}
