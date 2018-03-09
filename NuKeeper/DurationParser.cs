using System;

namespace NuKeeper
{
    public static class DurationParser
    {
        public static TimeSpan? Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            char suffix = value[value.Length - 1];
            var prefix = value.Substring(0, value.Length - 1);

            var parsed = int.TryParse(prefix, out int count);
            if (!parsed)
            {
                return null;
            }

            switch (suffix)
            {
                case 'h':
                    return TimeSpan.FromHours(count);

                case 'd':
                    return TimeSpan.FromDays(count);

                case 'w':
                    return TimeSpan.FromDays(count * 7);

                default:
                    return null;
            }
        }
    }
}
