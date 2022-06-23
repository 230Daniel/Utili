using System.Collections.Generic;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities
{
    public class MessageLogsConfiguration : GuildEntity
    {
        public ulong DeletedChannelId { get; set; }
        public ulong EditedChannelId { get; set; }
        public List<ulong> ExcludedChannels { get; set; }
        public bool LogThreads { get; set; }

        public MessageLogsConfiguration(ulong guildId) : base(guildId) { }
    }
}
