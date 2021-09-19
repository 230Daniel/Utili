using System.Collections.Generic;
using Database.Entities.Base;

namespace Database.Entities
{
    public class RolePersistConfiguration : GuildEntity
    {
        public bool Enabled { get; set; }
        public List<ulong> ExcludedRoles { get; set; }

        public RolePersistConfiguration(ulong guildId) : base(guildId) { }
    }
}
