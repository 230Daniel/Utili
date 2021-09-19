using System;
using Database.Entities.Base;

namespace Database.Entities
{
    public class AutopurgeConfiguration : GuildChannelEntity
    {
        public TimeSpan Timespan { get; set; }
        public AutopurgeMode Mode { get; set; }
        public bool AddedFromDashboard { get; set; }

        public AutopurgeConfiguration(ulong guildId, ulong channelId) : base(guildId, channelId) { }
    }

    public enum AutopurgeMode
    {
        All = 0,
        Bot = 1,
        User = 2,
        None = 3
    }
}
