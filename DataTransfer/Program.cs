using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using DataTransfer.Transfer;
using Discord.Rest;

namespace DataTransfer
{
    internal static class Program
    {
        public static List<IRow> RowsToSave;
        public static DiscordRestClient Client;

        static async Task Main()
        {
            V1Data.SetConnectionString(Menu.GetString("v1 connection"));
            V1Data.Cache = V1Data.GetDataWhere("DataType NOT LIKE '%RolePersist-Role-%'");

            await Database.Database.InitialiseAsync(false, "");

            Client = new DiscordRestClient();
            await Client.LoginAsync(Discord.TokenType.Bot, Menu.GetString("token"));

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
                    "Notices",
                    "All except inactive role users");
                
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
                        await Notices.TransferAsync(guildId);
                        break;

                    case 9:
                        await Autopurge.TransferAsync(guildId);
                        Console.WriteLine("Autopurge done");

                        await ChannelMirroring.TransferAsync(guildId);
                        Console.WriteLine("Channel Mirroring done");

                        await Core.TransferAsync(guildId);
                        Console.WriteLine("Core done");

                        await InactiveRole.TransferAsync(guildId);
                        Console.WriteLine("Inactive Role done");

                        await JoinMessage.TransferAsync(guildId);
                        Console.WriteLine("Join Message done");

                        await MessageFilter.TransferAsync(guildId);
                        Console.WriteLine("Message Filter done");

                        await MessageLogs.TransferAsync(guildId);
                        Console.WriteLine("Message Logs done");

                        await Notices.TransferAsync(guildId);
                        Console.WriteLine("Notices done");
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
