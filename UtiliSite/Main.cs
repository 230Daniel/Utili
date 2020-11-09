using Database;

namespace UtiliSite
{
    internal static class Main
    {
        public static Config _config;

        public static void Initialise()
        {
            _config = new Config();
            _config.Load();

            DiscordModule.Initialise();
        }
    }
}
