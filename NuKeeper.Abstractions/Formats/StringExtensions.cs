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

        public static bool ContainsOrdinal(this string str, string substr)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (substr == null)
            {
                throw new ArgumentNullException(nameof(substr));
            }

            return str.IndexOf(substr, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
