using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class VoiceRoleConfiguration : GuildChannelEntity
    {
        public ulong RoleId { get; set; }

        public VoiceRoleConfiguration(ulong guildId, ulong channelId) : base(guildId, channelId) { }
    }
}
