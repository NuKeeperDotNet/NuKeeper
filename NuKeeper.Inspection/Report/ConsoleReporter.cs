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
            Console.WriteLine(MessageForCount(updates.Count));
            Console.WriteLine(MessageForAgeSum(updates));
            Console.WriteLine();

            foreach (var update in updates)
            {
                Console.WriteLine(Describe(update));
            }
        }

        private string MessageForCount(int count)
        {
            if (count == 0)
            {
                return "Found no package updates";
            }
            else if (count == 1)
            {
                return "Found 1 package update";
            }

            return $"Found {count} package updates";
        }

        private string MessageForAgeSum(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var totalAge = Age.Sum(updates);
            var ageString = totalAge.ToString("%d");
            return $"Total package age:{ageString} days";
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

            var ago = "?";
            if (update.Selected.Published.HasValue)
            {
                var pubDate = update.Selected.Published.Value.UtcDateTime;
                ago = TimeSpanFormat.Ago(pubDate, DateTime.UtcNow);
            }

            var optS = occurences > 1 ? "s" : string.Empty;

            return  $"{update.SelectedId} to {update.SelectedVersion} from {versionInUse} in {occurences} place{optS} since {ago}.";
        }
    }
}
