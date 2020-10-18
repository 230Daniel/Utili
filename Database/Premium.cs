using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class Premium
    {
        public static bool IsPremium(ulong guildId)
        {
            return true;
        }

        public static List<ulong> GetPremiumGuilds()
        {
            return new List<ulong>
            {
                763298540591513643
            };
        }
    }
}
