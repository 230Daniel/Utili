using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UtiliSite.Pages
{
    public class Helper
    {
        public static string GetScript(string javascript)
        {
            return @$"<script type='text/javascript' language='javascript'> {javascript} </script>";
        }
    }
}
