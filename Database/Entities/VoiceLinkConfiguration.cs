using System.Collections.Generic;
using Database.Entities.Base;

namespace Database.Entities
{
    public class VoiceLinkConfiguration : GuildEntity
    {
        public bool Enabled { get; set; }
        public bool DeleteChannels { get; set; }
        public string ChannelPrefix { get; set; }
        public List<ulong> ExcludedChannels { get; set; }

        public VoiceLinkConfiguration(ulong guildId) : base(guildId) { }
    }
}
