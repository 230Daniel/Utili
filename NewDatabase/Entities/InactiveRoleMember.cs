using System;
using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class InactiveRoleMember : MemberEntity
    {
        public DateTime LastAction { get; set; }

        public InactiveRoleMember(ulong guildId, ulong memberId) : base(guildId, memberId) { }
    }
}
