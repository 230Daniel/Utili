using System.Collections.Generic;
using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class MessageLogsConfiguration : GuildEntity
    {
        public ulong DeletedChannelId { get; set; }
        public ulong EditedChannelId { get; set; }
        public List<ulong> ExcludedChannels { get; set; }

        public MessageLogsConfiguration(ulong guildId) : base(guildId) { }
    }
}
