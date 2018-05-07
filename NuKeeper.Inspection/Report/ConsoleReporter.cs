using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report
{
    public class ConsoleReporter : IAvailableUpdatesReporter
    {
        public void Report(string name, IReadOnlyCollection<PackageUpdateSet> updates)
        {
            Console.WriteLine();
            if (updates.Count == 0)
            {
                Console.WriteLine("No package updates found");
            }

            foreach (var update in updates)
            {
                Console.WriteLine(Describe(update));
            }
        }

        private string Describe(PackageUpdateSet update)
        {
            var occurences = update.CurrentPackages.Count;
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

            var pubDate = update.Selected.Published.Value.UtcDateTime;
            var ago = TimeSpanFormat.Ago(pubDate, DateTime.UtcNow);

            var optS = occurences > 1 ? "s" : string.Empty;

            return  $"{update.SelectedId} to {update.SelectedVersion} from {versionInUse} in {occurences} place{optS} since {ago}.";
        }
    }
}
