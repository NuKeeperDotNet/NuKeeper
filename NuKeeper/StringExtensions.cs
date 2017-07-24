using System.Collections.Generic;

namespace NuKeeper
{
    public static class StringExtensions
    {
        public static string JoinWithCommas(this IEnumerable<string> values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            return string.Join(", ", values);
        }
    }
}