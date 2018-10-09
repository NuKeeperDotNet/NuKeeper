using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet.Versioning;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report
{
    public class CsvFileReporter: IAvailableUpdatesReporter
    {
        private readonly IReportStreamSource _reportStreamSource;
        private readonly INuKeeperLogger _logger;

        public CsvFileReporter(IReportStreamSource reportStreamSource, INuKeeperLogger logger)
        {
            _reportStreamSource = reportStreamSource;
            _logger = logger;
        }

        public void Report(string name, IReadOnlyCollection<PackageUpdateSet> updates)
        {
            using (var writer = _reportStreamSource.GetStream(name))
            {
                _logger.Detailed($"writing {updates.Count} lines to report");
                WriteHeading(writer);

                foreach (var update in updates)
                {
                    WriteLine(writer, update);
                }

                writer.Close();
                _logger.Detailed("Report written");
            }
        }

        private static void WriteHeading(StreamWriter writer)
        {
            writer.WriteLine(
                "Package id,Package source," +
                "Usage count,Versions in use,Lowest version in use,Highest Version in use," +
                "Major version update,Major published date," +
                "Minor version update,Minor published date," +
                "Patch version update,Patch published date"
                );
        }

        private static void WriteLine(StreamWriter writer, PackageUpdateSet update)
        {
            var occurrences = update.CurrentPackages.Count;
            var versionsInUse = update.CurrentPackages
                .Select(p => p.Version)
                .ToList();

            var lowest = versionsInUse.Min();
            var highest = versionsInUse.Max();

            var packageSource = update.Selected.Source;

            var majorData = PackageVersionAndDate(lowest, update.Packages.Major);
            var minorData = PackageVersionAndDate(lowest, update.Packages.Minor);
            var patchData = PackageVersionAndDate(lowest, update.Packages.Patch);

            writer.WriteLine(
                $"{update.SelectedId},{packageSource}," +
                $"{occurrences},{update.CountCurrentVersions()},{lowest},{highest}," +
                $"{majorData},{minorData},{patchData}");
        }

        private static string PackageVersionAndDate(NuGetVersion baseline, PackageSearchMedatadata packageVersion)
        {
            const string none = ",";

            if (packageVersion == null)
            {
                return none;
            }

            if (packageVersion.Identity.Version <= baseline)
            {
                return none;
            }

            var version = packageVersion.Identity.Version;
            var date = DateFormat.AsUtcIso8601(packageVersion.Published);
            return $"{version},{date}";
        }
    }
}
