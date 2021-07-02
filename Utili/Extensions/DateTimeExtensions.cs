using System;

namespace Utili.Extensions
{
    internal static class DateTimeExtensions
    {
        public static string ToUniversalFormat(this DateTime dt)
        {
            return $"{dt.Year}-{dt.Month}-{dt.Day} {dt.Hour}:{dt.Minute}:{dt.Second}";
        }
    }
}