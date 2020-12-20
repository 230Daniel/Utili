namespace Database
{
    public static class Database
    {
        public static Config _config { get; set; }

        public static void Initialise(bool useCache, string defaultPrefix)
        {
            _config = new Config();
            _config.Load();
            _config.DefaultPrefix = defaultPrefix;

            Sql.SetCredentials(_config.Server, _config.Port, _config.Database, _config.Username, _config.Password);

            if (useCache)
            {
                Cache.Initialise();
            }
        }
    }
}
