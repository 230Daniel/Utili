using System.Collections.Generic;
using Database.Entities.Base;

namespace Database.Entities
{
    public class RolePersistMember : MemberEntity
    {
        public List<ulong> Roles { get; set; }

        public RolePersistMember(ulong guildId, ulong memberId) : base(guildId, memberId) { }
    }
}
