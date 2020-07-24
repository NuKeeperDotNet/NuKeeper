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

        // copied from netcoreapp2.1 profile
        public static bool Contains(
            this string subject,
            string value,
            StringComparison comparisonType)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(nameof(subject));
            }

            return subject.IndexOf(value, 0, subject.Length, comparisonType) >= 0;
        }
    }
}
