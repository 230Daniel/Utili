using System.Threading.Tasks;

namespace Database
{
    public static class Database
    {
        public static Config Config { get; private set; }

        public static async Task InitialiseAsync(bool useCache, string defaultPrefix, bool prod = false)
        {
            Config = new Config();
            Config.Load(prod);
            Config.DefaultPrefix = defaultPrefix;

            Sql.SetCredentials(Config.Server, Config.Port, Config.Database, Config.Username, Config.Password);
            await Sql.PingAsync();

            if (useCache) await Cache.InitialiseAsync();
            Data.Premium.Initialise();
            Data.MessageLogs.Initialise();
        }
    }
}
