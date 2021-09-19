namespace NewDatabase.Entities
{
    public class RoleLinkingConfiguration
    {
        public int Id { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong RoleId { get; set; }
        public ulong LinkedRoleId { get; set; }
        public RoleLinkingMode Mode { get; set; }

        public RoleLinkingConfiguration(ulong guildId)
        {
            GuildId = guildId;
        }
        
        internal RoleLinkingConfiguration() { }
    }

    public enum RoleLinkingMode
    {
        GrantOnGrant = 0,
        RevokeOnGrant = 1,
        GrantOnRevoke = 2,
        RevokeOnRevoke = 3
    }
}
