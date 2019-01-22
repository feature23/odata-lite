using System;

namespace F23.ODataLite.Internal
{
    internal static class StringExtensions
    {
        public static string[] Split(this string @string, char separator, StringSplitOptions options)
            => @string.Split(new[] { separator }, options);
    }
}
