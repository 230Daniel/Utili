using System.Collections.Generic;
using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class JoinRolesConfiguration : GuildEntity
    {
        public bool WaitForVerification { get; set; }
        public List<ulong> JoinRoles { get; set; }
        
        public JoinRolesConfiguration(ulong guildId) : base(guildId) { }
    }
}
