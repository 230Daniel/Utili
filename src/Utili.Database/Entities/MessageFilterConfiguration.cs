using System;
using Utili.Database.Entities.Base;

namespace Utili.Database.Entities
{
    public class MessageFilterConfiguration : GuildChannelEntity
    {
        public MessageFilterMode Mode { get; set; }
        public string RegEx { get; set; }
        public string DeletionMessage { get; set; }
        public bool EnforceInThreads { get; set; }

        public MessageFilterConfiguration(ulong guildId, ulong channelId) : base(guildId, channelId) { }
    }

    [Flags]
    public enum MessageFilterMode
    {
        All = 1,
        Images = 2,
        Videos = 4,
        Music = 8,
        Attachments = 16,
        Links = 32,
        RegEx = 64
    }
}
