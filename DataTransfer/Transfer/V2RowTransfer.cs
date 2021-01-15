using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataTransfer.Transfer
{
    internal static class V2RowTransfer
    {
        public static async Task TransferAsync(ulong guildId, string fromDatabase)
        {
            string[] tables = {
                "Autopurge",
                "ChannelMirroring",
                "Core",
                "InactiveRole",
                "InactiveRoleUsers",
                "JoinMessage",
                "JoinRoles",
                "MessageFilter",
                "MessageLogs",
                "MessageLogsMessages",
                "MessagePinning",
                "Misc",
                "Notices",
                "Reputation",
                "ReputationUsers",
                "RolePersist",
                "RolePersistRoles",
                "VoiceLink",
                "VoiceLinkChannels",
                "VoiceRoles",
                "VoteChannels"
            };

            List<Task> tasks = new List<Task>();
            foreach (string table in tables)
            {
                string command = @$"
                DELETE FROM {table} WHERE GuildId = @GuildId;
                INSERT INTO {table}
                SELECT *
                FROM {fromDatabase}.{table}
                WHERE GuildId = @GuildId;";

                tasks.Add(Database.Sql.ExecuteAsync(
                    command,
                    ("GuildId", guildId)));
            }

            await Task.WhenAll(tasks);
        }
    }
}
