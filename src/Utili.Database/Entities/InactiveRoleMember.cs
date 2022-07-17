using System;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities;

public class InactiveRoleMember : MemberEntity
{
    public DateTime LastAction { get; set; }

    public InactiveRoleMember(ulong guildId, ulong memberId) : base(guildId, memberId) { }
}