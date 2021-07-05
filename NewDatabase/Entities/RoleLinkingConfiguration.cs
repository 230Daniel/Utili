namespace NewDatabase.Entities
{
    public class RoleLinkingConfiguration
    {
        public int Id { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong RoleId { get; }
        public ulong LinkedRoleId { get; set; }
        public RoleLinkingMode Mode { get; set; }

        public RoleLinkingConfiguration(ulong guildId, ulong roleId)
        {
            GuildId = guildId;
            RoleId = roleId;
        }

        internal RoleLinkingConfiguration() { }
    }

    public enum RoleLinkingMode
    {
        GrantOnGrant,
        RevokeOnGrant,
        GrantOnRevoke,
        RevokeOnRevoke
    }
}
