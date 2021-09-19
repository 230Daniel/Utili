using System.Collections.Generic;
using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class RolePersistMember : MemberEntity
    {
        public List<ulong> Roles { get; set; }

        public RolePersistMember(ulong guildId, ulong memberId) : base(guildId, memberId) { }
    }
}
