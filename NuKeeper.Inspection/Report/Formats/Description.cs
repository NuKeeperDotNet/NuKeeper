using System;
using System.Linq;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.Report.Formats
{
    public static class Description
    {
        public static string ForUpdateSet(PackageUpdateSet update)
        {
            var occurrences = update.CurrentPackages.Count;
            var versionsInUse = update.CurrentPackages
                .Select(p => p.Version)
                .ToList();

            var lowest = versionsInUse.Min();
            var highest = versionsInUse.Max();

            string versionInUse;
            if (lowest == highest)
            {
                versionInUse = highest.ToString();
            }
            else
            {
                versionInUse = $"{lowest} - {highest}";
            }

            var ago = "?";
            if (update.Selected.Published.HasValue)
            {
                var pubDate = update.Selected.Published.Value.UtcDateTime;
                ago = TimeSpanFormat.Ago(pubDate, DateTime.UtcNow);
            }

            var optS = occurrences > 1 ? "s" : string.Empty;

            return $"{update.SelectedId} to {update.SelectedVersion} from {versionInUse} in {occurrences} place{optS} since {ago}.";
        }

        public static string ForDowngradeSet(PackageUpdateSet update)
        {
            var occurrences = update.CurrentPackages.Count;
            var versionsInUse = update.CurrentPackages
                .Select(p => p.Version)
                .ToList();

            var lowest = versionsInUse.Min();
            var highest = versionsInUse.Max();

            string versionInUse;
            if (lowest == highest)
            {
                versionInUse = highest.ToString();
            }
            else
            {
                versionInUse = $"{lowest} - {highest}";
            }

            var ago = "?";
            if (update.Selected.Published.HasValue)
            {
                var pubDate = update.Selected.Published.Value.UtcDateTime;
                ago = TimeSpanFormat.Ago(pubDate, DateTime.UtcNow);
            }

            var optS = occurrences > 1 ? "s" : string.Empty;

            return $"{update.SelectedId} to {versionInUse} from {update.SelectedVersion} in {occurrences} place{optS} since {ago}.";
        }

    }
}
