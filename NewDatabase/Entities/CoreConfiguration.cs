using System;
using System.Collections.Generic;
using NewDatabase.Entities.Base;

namespace NewDatabase.Entities
{
    public class CoreConfiguration : GuildEntity
    {
        public string Prefix { get; set; }
        public bool CommandsEnabled { get; set; }
        public List<ulong> NonCommandChannels { get; set; }
        public BotFeatures BotFeatures { get; set; }

        public CoreConfiguration(ulong guildId) : base(guildId) { }

        public bool HasAutopurge => (BotFeatures & BotFeatures.Autopurge) != 0;
        public bool HasChannelMirroring => (BotFeatures & BotFeatures.ChannelMirroring) != 0;
        public bool HasInactiveRole => (BotFeatures & BotFeatures.InactiveRole) != 0;
        public bool HasJoinMessage => (BotFeatures & BotFeatures.JoinMessage) != 0;
        public bool HasJoinRoles => (BotFeatures & BotFeatures.JoinRoles) != 0;
        public bool HasMessageFilter => (BotFeatures & BotFeatures.MessageFilter) != 0;
        public bool HasMessageLogs => (BotFeatures & BotFeatures.MessageLogs) != 0;
        public bool HasMessagePinning => (BotFeatures & BotFeatures.MessagePinning) != 0;
        public bool HasNotices => (BotFeatures & BotFeatures.Notices) != 0;
        public bool HasReputation => (BotFeatures & BotFeatures.Reputation) != 0;
        public bool HasRoleLinking => (BotFeatures & BotFeatures.RoleLinking) != 0;
        public bool HasRolePersist => (BotFeatures & BotFeatures.RolePersist) != 0;
        public bool HasVoiceLink => (BotFeatures & BotFeatures.VoiceLink) != 0;
        public bool HasVoiceRoles => (BotFeatures & BotFeatures.VoiceRoles) != 0;
        public bool HasVoteChannels => (BotFeatures & BotFeatures.VoteChannels) != 0;
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
