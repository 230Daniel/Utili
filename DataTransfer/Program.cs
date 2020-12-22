using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using DataTransfer.Transfer;
using Database.Data;

namespace DataTransfer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            V1Data.SetConnectionString(Menu.GetString("v1 connection"));
            await Database.Database.InitialiseAsync(false, "");

            Console.WriteLine("NonQuery");
            await Sql.ExecuteAsync(
                "INSERT INTO Test (Num, Dat) VALUES (@Num, @Dat);",
                ("Num", 23),
                ("Dat", DateTime.Now));

            await Task.Delay(5000);

            for(int i = 0; i < 5; i++)
            {
                int p = await Sql.PingAsync();
                Console.WriteLine($"pingback {p}ms");
                await Task.Delay(2000);
            }
            
            await Task.Delay(5000);

            Console.WriteLine("NonQuery");
            await Sql.ExecuteAsync(
                "INSERT INTO Test (Num, Dat) VALUES (@Num, @Dat);",
                ("Num", 23),
                ("Dat", DateTime.Now));

            await Task.Delay(5000);

            Console.WriteLine("Reader");
            var r = await Sql.ExecuteReaderAsync("SELECT * FROM INFORMATION_SCHEMA.PROCESSLIST WHERE HOST LIKE '%virgin%';");
            await Task.Delay(1000);
            r.Close();

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

                switch (Menu.PickOption("Inactive Role Users"))
                {
                    case 0:
                        InactiveRoleUsers.Transfer(guildId);
                        break;
                }

                Menu.Continue();
            }
        }
    }
}
