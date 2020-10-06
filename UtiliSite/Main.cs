using Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UtiliSite
{
    internal static class Main
    {
        public static Config _config;

        public static void Initialise()
        {
            _config = new Config();
            _config.Load();
        }
    }
}
