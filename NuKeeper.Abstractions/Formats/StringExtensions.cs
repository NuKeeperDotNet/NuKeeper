using System;
using System.Collections.Generic;

namespace NuKeeper.Abstractions.Formats
{
    public static class StringExtensions
    {
        public static string JoinWithCommas(this IEnumerable<string> values)
        {
            return JoinWithSeparator(values, ", ");
        }

        public static string JoinWithSeparator(this IEnumerable<string> values, string separator)
        {
            return values == null ? string.Empty : string.Join(separator, values);
        }

        public static bool ContainsOrdinal(this string value, string substring)
        {
            if (value == null)
            {
                return false;
            }

            if (substring == null)
            {
                throw new ArgumentNullException(nameof(substring));
            }

            return value.IndexOf(substring, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
