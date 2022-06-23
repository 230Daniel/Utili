using System.Collections.Generic;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities
{
    public class JoinRolesConfiguration : GuildEntity
    {
        public bool WaitForVerification { get; set; }
        public List<ulong> JoinRoles { get; set; }
        public bool CancelOnRolePersist { get; set; }

        public JoinRolesConfiguration(ulong guildId) : base(guildId) { }
    }
}
