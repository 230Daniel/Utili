using System.Threading.Tasks;

namespace UtiliSite
{
    internal static class Main
    {
        public static Config Config;

        public static async Task InitialiseAsync()
        {
            Config = new Config();
            Config.Load();

            await Database.Database.InitialiseAsync(false, Config.DefaultPrefix);

            await DiscordModule.InitialiseAsync();
        }
    }
}
