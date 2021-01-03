using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using DataTransfer.Transfer;
using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace DataTransfer
{
    internal static class Program
    {
        public static List<IRow> RowsToSave;
        public static DiscordSocketClient Client;

        static async Task Main()
        {
            V1Config config = V1Config.Load();

            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = false,
                ExclusiveBulkDelete = true,
                LogLevel = LogSeverity.Critical,
                MessageCacheSize = 100,
                GatewayIntents = 
                    GatewayIntents.GuildEmojis |
                    GatewayIntents.GuildMembers |
                    GatewayIntents.GuildMessageReactions |
                    GatewayIntents.GuildMessages |
                    GatewayIntents.Guilds |
                    GatewayIntents.GuildVoiceStates |
                    GatewayIntents.GuildWebhooks
            });

            Console.WriteLine("Starting bot...");
            await Client.LoginAsync(TokenType.Bot, config.V1Token);
            _ = Client.StartAsync();

            Console.WriteLine("Downloading v1 cache...");
            V1Data.SetConnectionString(config.V1Connection);
            V1Data.Cache = V1Data.GetDataWhere("DataType NOT LIKE '%RolePersist-Role-%'");

            Console.WriteLine("Logging in to v2...");
            await Database.Database.InitialiseAsync(false, "");

            Console.WriteLine("Waiting for bot...");
            while (Client.ConnectionState != ConnectionState.Connected) await Task.Delay(1000);

            while (true)
            {
                ulong? guildId = null;
                RowsToSave = new List<IRow>();

                switch (Menu.PickOption("All guilds", "One guild"))
                {
                    case 0:
                        break;

                    case 1:
                        guildId = Menu.GetUlong("guild");
                        break;
                }

                int option = Menu.PickOption(
                    "Autopurge", 
                    "Channel Mirroring", 
                    "Core", 
                    "Inactive Role", 
                    "Inactive Role Users",
                    "Join Message",
                    "Message Filter",
                    "Message Logs",
                    "Message Logs Messages",
                    "Notices",
                    "Roles",
                    "Roles Persist Roles",
                    "Voice Link",
                    "Voice Roles",
                    "Vote Channels",
                    "All except message logs messages and inactive role users");
                
                Console.Clear();
                Console.WriteLine("Getting data...");

                switch (option)
                {
                    case 0:
                        await Autopurge.TransferAsync(guildId);
                        break;

                    case 1:
                        await ChannelMirroring.TransferAsync(guildId);
                        break;

                    case 2:
                        await Core.TransferAsync(guildId);
                        break;
                        
                    case 3:
                        await InactiveRole.TransferAsync(guildId);
                        break;

                    case 4:
                        await InactiveRoleUsers.TransferAsync(guildId);
                        break;

                    case 5:
                        await JoinMessage.TransferAsync(guildId);
                        break;

                    case 6:
                        await MessageFilter.TransferAsync(guildId);
                        break;

                    case 7:
                        await MessageLogs.TransferAsync(guildId);
                        break;
                        
                    case 8:
                        await MessageLogsMessages.TransferAsync(guildId);
                        break;

                    case 9:
                        await Notices.TransferAsync(guildId);
                        break;

                    case 10:
                        await Roles.TransferAsync(guildId);
                        break;

                    case 11:
                        await RolesPersistRoles.TransferAsync(guildId);
                        break;

                    case 12:
                        await VoiceLink.TransferAsync(guildId);
                        break;

                    case 13:
                        await VoiceRoles.TransferAsync(guildId);
                        break;

                    case 14:
                        await VoteChannels.TransferAsync(guildId);
                        break;

                    case 15:

                        Console.WriteLine("Autopurge...");
                        await Autopurge.TransferAsync(guildId);

                        Console.WriteLine("Channel Mirroring...");
                        await ChannelMirroring.TransferAsync(guildId);
                        
                        Console.WriteLine("Core...");
                        await Core.TransferAsync(guildId);
                        
                        Console.WriteLine("Inactive Role...");
                        await InactiveRole.TransferAsync(guildId);
                        
                        Console.WriteLine("Join Message...");
                        await JoinMessage.TransferAsync(guildId);
                        
                        Console.WriteLine("Message Filter...");
                        await MessageFilter.TransferAsync(guildId);
                        
                        Console.WriteLine("Message Logs...");
                        await MessageLogs.TransferAsync(guildId);
                        
                        Console.WriteLine("Notices...");
                        await Notices.TransferAsync(guildId);

                        Console.WriteLine("Roles...");
                        await Roles.TransferAsync(guildId);

                        Console.WriteLine("Roles Persist Roles...");
                        await RolesPersistRoles.TransferAsync(guildId);

                        Console.WriteLine("Voice link...");
                        await VoiceLink.TransferAsync(guildId);

                        Console.WriteLine("Voice roles...");
                        await VoiceRoles.TransferAsync(guildId);

                        Console.WriteLine("Vote Channels...");
                        await VoteChannels.TransferAsync(guildId);

                        break;
                }

                Console.WriteLine("Starting the transfer...");
                RowsToSave.ForEach(x => x.New = true);
                List<Task> tasks = RowsToSave.Select(SaveRow).ToList();

                Console.WriteLine("Inserting rows...");
                while (tasks.Any(x => !x.IsCompleted))
                {
                    Console.WriteLine($"{tasks.Count(x => x.IsCompleted)} / {tasks.Count}");
                    await Task.Delay(1000);
                }

                await Task.WhenAll(tasks);
                Console.WriteLine($"{tasks.Count(x => x.IsCompleted)} / {tasks.Count}");
                Console.WriteLine("Done");

                Menu.Continue();
            }
        }

        private static async Task SaveRow(IRow row)
        {
            await Task.Delay(1);
            try
            {
                await row.SaveAsync();
            }
            catch { }
        }
    }
}
