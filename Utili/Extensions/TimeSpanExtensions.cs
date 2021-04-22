using System;

namespace Utili.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToLongString (this TimeSpan span)
        {
            string formatted =
                $"{(span.Duration().Days > 0 ? $"{span.Days:0} day{(span.Days == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                $"{(span.Duration().Hours > 0 ? $"{span.Hours:0} hour{(span.Hours == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                $"{(span.Duration().Minutes > 0 ? $"{span.Minutes:0} minute{(span.Minutes == 1 ? string.Empty : "s")}, " : string.Empty)}" +
                $"{(span.Duration().Seconds > 0 ? $"{span.Seconds:0} second{(span.Seconds == 1 ? string.Empty : "s")}" : string.Empty)}";

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        public static string ToShortString (this TimeSpan span)
        {
            string formatted =
                $"{(span.Duration().Days > 0 ? $"{span.Days:00}:" : string.Empty)}{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "00:00:00";

            return formatted;
        }
    }
}
