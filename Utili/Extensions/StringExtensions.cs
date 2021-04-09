using System;
using System.Linq;

namespace DisqordTestBot.Extensions
{
    public static class StringExtensions
    {
        public static string Title(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1)
            };
    }
}
