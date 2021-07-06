using System;
using System.Collections.Generic;
using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class VoteChannelConfiguration : GuildChannelEntity
    {
        public VoteChannelMode Mode { get; set; }
        public List<string> Emotes { get; set; }

        public VoteChannelConfiguration(ulong guildId, ulong channelId) : base(guildId, channelId) { }
    }
    
    [Flags]
    public enum VoteChannelMode
    {
        All = 1,
        Images = 2,
        Videos = 4,
        Music = 8,
        Attachments = 16,
        Links = 32,
        Embeds = 64
    }
}
