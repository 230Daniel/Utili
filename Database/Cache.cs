using System.Collections.Generic;
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

        public static List<AutopurgeRow> Autopurge { get; set; }
        public static List<ChannelMirroringRow> ChannelMirroring { get; set; }
        public static List<CoreRow> Core { get; set; }
        public static List<InactiveRoleRow> InactiveRole { get; set; }
        public static List<JoinMessageRow> JoinMessage { get; set; }
        public static List<MessageFilterRow> MessageFilter { get; set; }
        public static List<MessageLogsRow> MessageLogs { get; set; }
        public static List<MessagePinningRow> MessagePinning { get; set; }
        public static List<MiscRow> Misc { get; set; }
        public static List<NoticesRow> Notices { get; set; }
        public static List<PremiumRow> Premium { get; set; }
        public static List<ReputationRow> Reputation { get; set; }
        public static List<RolesRow> Roles { get; set; }
        public static List<VoiceLinkRow> VoiceLink { get; set; }
        public static List<VoiceLinkChannelRow> VoiceLinkChannels { get; set; }
        public static List<VoiceRolesRow> VoiceRoles { get; set; }
        public static List<VoteChannelsRow> VoteChannels { get; set; }
        
        public static async Task InitialiseAsync()
        {
            await DownloadCacheAsync();

            Timer?.Dispose();
            Timer = new Timer(30000);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();

            Initialised = true;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = DownloadCacheAsync();
        }

        private static async Task DownloadCacheAsync()
        {
            Autopurge = await Data.Autopurge.GetRowsAsync(ignoreCache: true);
            ChannelMirroring = await Data.ChannelMirroring.GetRowsAsync(ignoreCache: true);
            Core = await Data.Core.GetRowsAsync(ignoreCache: true);
            InactiveRole = await Data.InactiveRole.GetRowsAsync(ignoreCache: true);
            JoinMessage = await Data.JoinMessage.GetRowsAsync(ignoreCache: true);
            MessageFilter = await Data.MessageFilter.GetRowsAsync(ignoreCache: true);
            MessageLogs = await Data.MessageLogs.GetRowsAsync(ignoreCache: true);
            Misc = await Data.Misc.GetRowsAsync(ignoreCache: true);
            Notices = await Data.Notices.GetRowsAsync(ignoreCache: true);
            Premium = await Data.Premium.GetRowsAsync(ignoreCache: true);
            Reputation = await Data.Reputation.GetRowsAsync(ignoreCache: true);
            Roles = await Data.Roles.GetRowsAsync(ignoreCache: true);
            VoiceLink = await Data.VoiceLink.GetRowsAsync(ignoreCache: true);
            VoiceLinkChannels = await Data.VoiceLink.GetChannelRowsAsync(ignoreCache: true);
            VoiceRoles = await Data.VoiceRoles.GetRowsAsync(ignoreCache: true);
            VoteChannels = await Data.VoteChannels.GetRowsAsync(ignoreCache: true);
        }
    }
}
