using System;
using System.Threading.Tasks;
using Database;

namespace UtiliSite
{
    internal static class Main
    {
        public static Config Config;

        public static async Task InitialiseAsync()
        {
            _config = Config.Load();
            PaymentsController.Initialise();
            Database.Database.Initialise(false);
            await DiscordModule.InitialiseAsync();
        }
    }
}
