using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public static class Main
    {
        private static Config _config;

        public static void Initialise()
        {
            _config = new Config();
            _config.Load();

            Sql.SetCredentials(_config.Server, _config.Database, _config.Username, _config.Password);

            Cache.Initialise();
        }
    }
}
