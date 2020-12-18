using System.Threading.Tasks;
using Database;

namespace UtiliSite
{
    internal static class Main
    {
        public static Config _config;

        public static async Task InitialiseAsync()
        {
            _config = new Config();
            _config.Load();

            Database.Database.Initialise(false);

            await DiscordModule.InitialiseAsync();
        }
    }
}
