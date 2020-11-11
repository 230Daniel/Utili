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
        public static bool Initialised;
        private static Timer Timer { get; set; }

        public static AutopurgeTable Autopurge { get; set; } = new AutopurgeTable();
        public static ChannelMirroringTable ChannelMirroring { get; set; } = new ChannelMirroringTable();
        public static InactiveRoleTable InactiveRole { get; set; } = new InactiveRoleTable();
        public static MessageFilterTable MessageFilter { get; set; } = new MessageFilterTable();
        public static MessageLogsTable MessageLogs { get; set; } = new MessageLogsTable();
        public static MiscTable Misc { get; set; } = new MiscTable();
        public static VoiceLinkTable VoiceLink { get; set; } = new VoiceLinkTable();
        public static VoiceRolesTable VoiceRoles { get; set; } = new VoiceRolesTable();
        public static VoteChannelsTable VoteChannels { get; set; } = new VoteChannelsTable();
        
        public static void Initialise() 
        // Start the automatic cache downloads
        {
            DownloadTables();

            Timer = new Timer(30000); // The cache will be updated every 30 seconds.
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

            Initialised = true;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        // At regular intervals, call the download tables method.
        {
            DownloadTables();
        }

        private static void DownloadTables()
        {
            Autopurge.Load();
            ChannelMirroring.Load();
            InactiveRole.Load();
            MessageFilter.Load();
            MessageLogs.Load();
            Misc.Load();
            VoiceLink.Load();
            VoiceRoles.Load();
            VoteChannels.Load();
        }
    }
}
