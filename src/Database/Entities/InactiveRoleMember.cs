using System;
using Database.Entities.Base;

namespace Database.Entities
{
    public class InactiveRoleMember : MemberEntity
    {
        public DateTime LastAction { get; set; }

        public InactiveRoleMember(ulong guildId, ulong memberId) : base(guildId, memberId) { }
    }
}
