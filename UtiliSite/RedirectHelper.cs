using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UtiliSite
{
    public class RedirectHelper
    {
        public static string AddToUrl(string path, string page)
        {
            if (!path.EndsWith("/")) path += "/";
            return path + page;
        }
    }
}
