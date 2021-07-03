using System.Collections.Generic;
using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class CoreConfiguration : GuildEntity
    {
        public string Prefix { get; set; }
        public bool CommandsEnabled { get; set; }
        public List<ulong> NonCommandChannels { get; set; }

        public CoreConfiguration(ulong guildId) : base(guildId) { }
    }
}
