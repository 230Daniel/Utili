using System.Collections.Generic;
using Database.Entities.Base;

namespace Database.Entities
{
    public class JoinRolesConfiguration : GuildEntity
    {
        public bool WaitForVerification { get; set; }
        public List<ulong> JoinRoles { get; set; }
        public bool CancelOnRolePersist { get; set; }

        public JoinRolesConfiguration(ulong guildId) : base(guildId) { }
    }
}
