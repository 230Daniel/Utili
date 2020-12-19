using System;
using DataTransfer.Transfer;

namespace DataTransfer
{
    class Program
    {
        static void Main(string[] args)
        {
            V1Data.SetConnectionString(Menu.GetString("v1 connection"));
            Database.Database.Initialise(false);

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
