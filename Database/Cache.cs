using System.Threading.Tasks;
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
        public static PremiumTable Premium { get; set; } = new PremiumTable();
        public static ReputationTable Reputation { get; set; } = new ReputationTable();
        public static RolesTable Roles { get; set; } = new RolesTable();
        public static VoiceLinkTable VoiceLink { get; set; } = new VoiceLinkTable();
        public static VoiceRolesTable VoiceRoles { get; set; } = new VoiceRolesTable();
        public static VoteChannelsTable VoteChannels { get; set; } = new VoteChannelsTable();
        
        public static void Initialise()
        {
            DownloadTables().GetAwaiter().GetResult();

            Timer?.Dispose();
            Timer = new Timer(30000);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

            Initialised = true;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = DownloadTables();
        }

        private static async Task DownloadTables()
        {
            Autopurge.Rows = await Data.Autopurge.GetRowsAsync(ignoreCache: true);
            ChannelMirroring.Rows = await Data.ChannelMirroring.GetRowsAsync(ignoreCache: true);
            Core.Rows = await Data.Core.GetRowsAsync(ignoreCache: true);
            InactiveRole.Rows = await Data.InactiveRole.GetRowsAsync(ignoreCache: true);
            JoinMessage.Rows = await Data.JoinMessage.GetRowsAsync(ignoreCache: true);
            MessageFilter.Rows = await Data.MessageFilter.GetRowsAsync(ignoreCache: true);
            MessageLogs.Rows = await Data.MessageLogs.GetRowsAsync(ignoreCache: true);
            Misc.Rows = await Data.Misc.GetRowsAsync(ignoreCache: true);
            Notices.Rows = await Data.Notices.GetRowsAsync(ignoreCache: true);
            Reputation.Rows = await Data.Reputation.GetRowsAsync(ignoreCache: true);
            Roles.Rows = await Data.Roles.GetRowsAsync(ignoreCache: true);
            VoiceLink.Rows = await Data.VoiceLink.GetRowsAsync(ignoreCache: true);
            VoiceLink.Channels = await Data.VoiceLink.GetChannelRowsAsync(ignoreCache: true);
            VoiceRoles.Rows = await Data.VoiceRoles.GetRowsAsync(ignoreCache: true);
            VoteChannels.Rows = await Data.VoteChannels.GetRowsAsync(ignoreCache: true);
        }
    }
}
