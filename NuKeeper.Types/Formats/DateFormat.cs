using System;
using System.Globalization;

namespace NuKeeper.Types.Formats
{
    public static class DateFormat
    {
        private const string Iso8601Format = "yyyy-MM-ddTHH\\:mm\\:ss";

        public static string AsUtcIso8601(DateTimeOffset? source)
        {
            if (!source.HasValue)
            {
                return string.Empty;
            }
            var utcValue = source.Value.ToUniversalTime();
            return string.Concat(utcValue.ToString(Iso8601Format, CultureInfo.InvariantCulture), "Z");
        }

    }
}
