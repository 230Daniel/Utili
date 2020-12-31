using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using DataTransfer.Transfer;

namespace DataTransfer
{
    internal static class Program
    {
        public static List<IRow> RowsToSave;

        static async Task Main()
        {
            V1Data.SetConnectionString(Menu.GetString("v1 connection"));
            V1Data.Cache = V1Data.GetDataWhere("DataType NOT LIKE '%RolePersist-Role-%'");

            await Database.Database.InitialiseAsync(false, "");
            RowsToSave = new List<IRow>();

            while (true)
            {
                ulong? guildId = null;

                switch (Menu.PickOption("All guilds", "One guild"))
                {
                    case 0:
                        break;

                    case 1:
                        guildId = Menu.GetUlong("guild");
                        break;
                }

                switch (Menu.PickOption("Autopurge", "Inactive Role Users"))
                {
                    case 0:
                        await Autopurge.TransferAsync(guildId);
                        break;

                    case 1:
                        await InactiveRoleUsers.TransferAsync(guildId);
                        break;
                }

                Console.Clear();
                Console.WriteLine("Inserting to v2...");
                RowsToSave.ForEach(x => x.New = false);
                List<Task> tasks = RowsToSave.Select(SaveRow).ToList();

                while (tasks.Any(x => !x.IsCompleted))
                {
                    Console.WriteLine($"{tasks.Count(x => x.IsCompleted)} / {tasks.Count}");
                    await Task.Delay(1000);
                }

                await Task.WhenAll(tasks);
                Console.WriteLine("Done");

                Menu.Continue();
            }
        }

        private static async Task SaveRow(IRow row)
        {
            //await Task.Delay(1);
            try
            {
                await row.SaveAsync();
            }
            catch { }
        }
    }
}
