using System;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities
{
    public class JoinRolesPendingMember : MemberEntity
    {
        public bool IsPending { get; set; }
        public DateTime ScheduledFor { get; set; }

        public JoinRolesPendingMember(ulong guildId, ulong memberId) : base(guildId, memberId) { }
    }
}
