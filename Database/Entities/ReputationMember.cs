﻿using Database.Entities.Base;

namespace Database.Entities
{
    public class ReputationMember : MemberEntity
    {
        public long Reputation { get; set; }

        public ReputationMember(ulong guildId, ulong memberId) : base(guildId, memberId) { }
    }
}
