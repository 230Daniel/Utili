using System.Collections.Generic;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities;

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