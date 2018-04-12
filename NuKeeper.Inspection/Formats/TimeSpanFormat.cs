using System;

namespace NuKeeper.Inspection.Formats
{
    public static class TimeSpanFormat
    {
        public static string Ago(DateTime start, DateTime end)
        {
            var duration = end.Subtract(start);
            if (start.Year == end.Year && start.Month == end.Month)
            {
                return Ago(duration);
            }

            if (duration.TotalDays < 29)
            {
                // no exact size for "a month", but this is a lower bound
                return Ago(duration);
            }

            int months = MonthsBetween(start, end);
            var years = months / 12;
            var remainderMonth = months % 12;

            if (years == 0)
            {
                return Plural(remainderMonth, "month") + " ago";
            }

            if (remainderMonth == 0)
            {
                return Plural(years, "year") + " ago";
            }

            return Plural(years, "year") + " and " + Plural(remainderMonth, "month") + " ago";
        }

        private static int MonthsBetween(DateTime start, DateTime end)
        {
            if (start.Year == end.Year)
            {
                return end.Month - start.Month;
            }

            var fullYears = (end.Year - start.Year - 1);
            var months = end.Month + 12 - start.Month;
            return fullYears * 12 + months;
        }

        public static string Ago(TimeSpan ago)
        {
            if (ago.TotalSeconds < 1)
            {
                return "now";
            }

            if (ago.TotalMinutes < 1)
            {
                var secs = (int)ago.TotalSeconds;
                return Plural(secs, "second") + " ago";
            }

            if (ago.TotalHours < 1)
            {
                var mins = (int)ago.TotalMinutes;
                return Plural(mins, "minute") + " ago";
            }

            if (ago.TotalDays < 1)
            {
                var hours = (int)ago.TotalHours;
                return Plural(hours, "hour") + " ago";
            }

            var days = (int)ago.TotalDays;
            return Plural(days, "day") + " ago";
        }

        private static string Plural(int value, string metric)
        {
            if (value == 1)
            {
                return $"1 {metric}";
            }

            return $"{value} {metric}s";
        }
    }
}
