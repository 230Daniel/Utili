using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class Premium
    {
        public static bool IsPremium(ulong guildId)
        {
            return new List<ulong>
            {
                611527825929011231
            }.Contains(guildId);
        }

        public static List<ulong> GetPremiumGuilds()
        {
            return new List<ulong>
            {
                611527825929011231
            };
        }
    }
}
