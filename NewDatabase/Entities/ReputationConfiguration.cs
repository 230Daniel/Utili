using System.Collections.Generic;
using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class ReputationConfiguration : GuildEntity
    {
        public List<ReputationConfigurationEmoji> Emojis { get; set; }

        public ReputationConfiguration(ulong guildId) : base(guildId) { }
    }

    public class ReputationConfigurationEmoji
    {
        public string Emoji { get; }
        public int Value { get; set; }

        public ReputationConfigurationEmoji(string emoji)
        {
            Emoji = emoji;
        }
    }
}
