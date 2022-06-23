using System;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities
{
    public class NoticeConfiguration : GuildChannelEntity
    {
        public bool Enabled { get; set; }
        public TimeSpan Delay { get; set; }
        public bool Pin { get; set; }
        public string Title { get; set; }
        public string Footer { get; set; }
        public string Content { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
        public string Thumbnail { get; set; }
        public string Icon { get; set; }
        public uint Colour { get; set; }
        public ulong MessageId { get; set; }
        public bool UpdatedFromDashboard { get; set; }

        public NoticeConfiguration(ulong guildId, ulong channelId) : base(guildId, channelId) { }
    }
}
