using System.Collections.Generic;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities;

public class RolePersistMember : MemberEntity
{
    public List<ulong> Roles { get; set; }

    public RolePersistMember(ulong guildId, ulong memberId) : base(guildId, memberId) { }
}