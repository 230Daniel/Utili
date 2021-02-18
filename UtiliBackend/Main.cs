using System.Threading.Tasks;

namespace UtiliBackend
{
    internal static class Main
    {
        public static Config Config;

        public static async Task InitialiseAsync()
        {
            Config = Config.Load();
            await Database.Database.InitialiseAsync(false, Config.DefaultPrefix);
            await DiscordModule.InitialiseAsync();
        }
    }
}
