using Utili.Database.Entities.Base;

namespace Utili.Database.Entities
{
    public class VoiceRoleConfiguration : GuildChannelEntity
    {
        public ulong RoleId { get; set; }

        public VoiceRoleConfiguration(ulong guildId, ulong channelId) : base(guildId, channelId) { }
    }
}
