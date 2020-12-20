using System.Timers;
using Database.Data;

namespace Database
{
    /*
     * The Cache class is responsible for downloading the data from the database and
     * returning it when requested. Overall, this should reduce the average latency
     * for fetching data from the database.
     */

    internal static class Cache
    {
        public static bool Initialised { get; private set; }
        private static Timer Timer { get; set; }

        public static AutopurgeTable Autopurge { get; set; } = new AutopurgeTable();
        public static ChannelMirroringTable ChannelMirroring { get; set; } = new ChannelMirroringTable();
        public static CoreTable Core { get; set; } = new CoreTable();
        public static InactiveRoleTable InactiveRole { get; set; } = new InactiveRoleTable();
        public static JoinMessageTable JoinMessage { get; set; } = new JoinMessageTable();
        public static MessageFilterTable MessageFilter { get; set; } = new MessageFilterTable();
        public static MessageLogsTable MessageLogs { get; set; } = new MessageLogsTable();
        public static MessagePinningTable MessagePinning { get; set; } = new MessagePinningTable();
        public static MiscTable Misc { get; set; } = new MiscTable();
        public static NoticesTable Notices { get; set; } = new NoticesTable();
        public static ReputationTable Reputation { get; set; } = new ReputationTable();
        public static RolesTable Roles { get; set; } = new RolesTable();
        public static VoiceLinkTable VoiceLink { get; set; } = new VoiceLinkTable();
        public static VoiceRolesTable VoiceRoles { get; set; } = new VoiceRolesTable();
        public static VoteChannelsTable VoteChannels { get; set; } = new VoteChannelsTable();
        
        public static void Initialise()
        {
            DownloadTables();

            Timer = new Timer(30000);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

            Initialised = true;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DownloadTables();
        }

        private static void DownloadTables()
        {
            Autopurge.Rows = Data.Autopurge.GetRows(ignoreCache: true);
            ChannelMirroring.Rows = Data.ChannelMirroring.GetRows(ignoreCache: true);
            Core.Rows = Data.Core.GetRows(ignoreCache: true);
            InactiveRole.Rows = Data.InactiveRole.GetRows(ignoreCache: true);
            JoinMessage.Rows = Data.JoinMessage.GetRows(ignoreCache: true);
            MessageFilter.Rows = Data.MessageFilter.GetRows(ignoreCache: true);
            MessageLogs.Rows = Data.MessageLogs.GetRows(ignoreCache: true);
            Misc.Rows = Data.Misc.GetRows(ignoreCache: true);
            Notices.Rows = Data.Notices.GetRows(ignoreCache: true);
            Reputation.Rows = Data.Reputation.GetRows(ignoreCache: true);
            Roles.Rows = Data.Roles.GetRows(ignoreCache: true);
            VoiceLink.Rows = Data.VoiceLink.GetMetaRows(ignoreCache: true);
            VoiceLink.Channels = Data.VoiceLink.GetChannelRows(ignoreCache: true);
            VoiceRoles.Rows = Data.VoiceRoles.GetRows(ignoreCache: true);
            VoteChannels.Rows = Data.VoteChannels.GetRows(ignoreCache: true);
        }
    }
}
