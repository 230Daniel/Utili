using System;
using System.Collections.Generic;
using Database.Entities.Base;

namespace Database.Entities
{
    public class CoreConfiguration : GuildEntity
    {
        public string Prefix { get; set; }
        public bool CommandsEnabled { get; set; }
        public List<ulong> NonCommandChannels { get; set; }
        public BotFeatures BotFeatures { get; set; }

        public CoreConfiguration(ulong guildId) : base(guildId) { }

        public bool HasFeature(BotFeatures feature)
        {
            return (BotFeatures & feature) != 0;
        }

        public void SetHasFeature(BotFeatures feature, bool enabled)
        {
            if (enabled) BotFeatures |= feature;
            else BotFeatures &= ~feature;
        }
    }

    [Flags]
    public enum BotFeatures
    {
        Autopurge = 1,
        ChannelMirroring = 2,
        InactiveRole = 4,
        JoinMessage = 8,
        JoinRoles = 16,
        MessageFilter = 32,
        MessageLogs = 64,
        MessagePinning = 128,
        Notices = 256,
        Reputation = 512,
        RoleLinking = 1024,
        RolePersist = 2048,
        VoiceLink = 4096,
        VoiceRoles = 8192,
        VoteChannels = 16384
    }
}
