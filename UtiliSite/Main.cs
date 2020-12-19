using System.Threading.Tasks;
using Database;

namespace UtiliSite
{
    internal static class Main
    {
        public static Config _config;

        public static async Task InitialiseAsync()
        {
            _config = Config.Load();
            PaymentsController.Initialise();
            Database.Database.Initialise(false);
            await DiscordModule.InitialiseAsync();
        }
    }
}
