using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report
{
    public class PlainTextReportFormat : IReportFormat
    {
        private readonly IReportWriter _writer;

        public PlainTextReportFormat(IReportWriter writer)
        {
            _writer = writer;
        }

        public void Write(string name, IReadOnlyCollection<PackageUpdateSet> updates)
        {
            _writer.WriteLine();
            _writer.WriteLine(MessageForCount(updates.Count));
            _writer.WriteLine(MessageForAgeSum(updates));
            _writer.WriteLine();

            foreach (var update in updates)
            {
                _writer.WriteLine(Describe(update));
            }
        }

        private string MessageForCount(int count)
        {
            if (count == 0)
            {
                return "Found no package updates";
            }
            if (count == 1)
            {
                return "Found 1 package update";
            }

            return $"Found {count} package updates";
        }

        private string MessageForAgeSum(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var totalAge = Age.Sum(updates);
            var years = totalAge.TotalDays / 365;

            var result =
             "Total package age:\n" +
             $" Days: {totalAge:%d}\n" +
             $" LibYears: {years:0.0}";
            return result;
        }

        public static string Describe(PackageUpdateSet update)
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

            return  $"{update.SelectedId} to {update.SelectedVersion} from {versionInUse} in {occurrences} place{optS} since {ago}.";
        }
    }
}
