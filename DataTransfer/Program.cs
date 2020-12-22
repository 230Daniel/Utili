using System.Threading.Tasks;
using DataTransfer.Transfer;

namespace DataTransfer
{
    internal static class Program
    {
        static async Task Main()
        {
            V1Data.SetConnectionString(Menu.GetString("v1 connection"));
            await Database.Database.InitialiseAsync(false, "");

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
                        await InactiveRoleUsers.TransferAsync(guildId);
                        break;
                }

                Menu.Continue();
            }
        }
    }
}
