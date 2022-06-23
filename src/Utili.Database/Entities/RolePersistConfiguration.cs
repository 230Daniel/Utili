using System.Collections.Generic;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities
{
    public class RolePersistConfiguration : GuildEntity
    {
        public bool Enabled { get; set; }
        public List<ulong> ExcludedRoles { get; set; }

        public RolePersistConfiguration(ulong guildId) : base(guildId) { }
    }
}
