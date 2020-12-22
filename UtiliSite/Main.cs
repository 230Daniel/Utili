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

            await Database.Database.InitialiseAsync(false, _config.DefaultPrefix);

            await DiscordModule.InitialiseAsync();
        }
    }
}
