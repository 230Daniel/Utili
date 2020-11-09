namespace Database
{
    public static class Database
    {
        private static Config _config;

        public static void Initialise(bool useCache)
        {
            _config = new Config();
            _config.Load();

            Sql.SetCredentials(_config.Server, _config.Port, _config.Database, _config.Username, _config.Password);

            if (useCache)
            {
                Cache.Initialise();
            }
        }
    }
}
