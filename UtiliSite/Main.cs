using System.Threading.Tasks;

namespace UtiliSite
{
    internal static class Main
    {
        public static Config Config;

        public static async Task InitialiseAsync()
        {
            Config = Config.Load();
            PaymentsController.Initialise();
            await Database.Database.InitialiseAsync(false, Config.DefaultPrefix);
            await DiscordModule.InitialiseAsync();
        }
    }
}
