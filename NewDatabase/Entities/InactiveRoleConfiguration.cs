using System;
using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class InactiveRoleConfiguration : GuildEntity
    {
        public ulong RoleId { get; set; }
        public ulong ImmuneRoleId { get; set; }
        public TimeSpan Threshold { get; set; }
        public InactiveRoleMode Mode { get; set; }
        public bool AutoKick { get; set; }
        public TimeSpan AutoKickThreshold { get; set; }
        public DateTime DefaultLastAction { get; set; }
        public DateTime LastUpdate { get; set; }

        public InactiveRoleConfiguration(ulong guildId) : base(guildId) { }
    }

    public enum InactiveRoleMode
    {
        GrantWhenInactive,
        RevokeWhenInactive
    }
}
