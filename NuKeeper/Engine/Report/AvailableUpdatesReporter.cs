using NuKeeper.RepositoryInspection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;

namespace NuKeeper.Engine.Report
{
    public class AvailableUpdatesReporter: IAvailableUpdatesReporter
    {
        private readonly INuKeeperLogger _logger;

        public AvailableUpdatesReporter(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public void Report(string name, List<PackageUpdateSet> updates)
        {
            using (var writer = MakeOutputStream(name))
            {
                _logger.Verbose($"writing {updates.Count} lines to report");
                WriteHeading(writer);

                foreach (var update in updates)
                {
                    WriteLine(writer, update);
                }

                writer.Close();
                _logger.Verbose("Report written");
            }
        }

        private void WriteHeading(StreamWriter writer)
        {
            writer.WriteLine(
                "Package id,Package source," +
                "Usage count,Versions in use,Lowest version in use,Highest Version in use," +
                "Major version update,Major published date," +
                "Minor version update,Minor published date," +
                "Patch version update,Patch published date,"
                );
        }

        private void WriteLine(StreamWriter writer, PackageUpdateSet update)
        {
            var occurences = update.CurrentPackages.Count;
            var versionsInUse = update.CurrentPackages
                .Select(p => p.Version)
                .ToList();

            var lowest = versionsInUse.Min();
            var highest = versionsInUse.Max();

            var packageSource = update.Selected.Source;

            var majorData = PackageVersionAndDate(update.Packages.Major);
            var minorData = PackageVersionAndDate(update.Packages.Minor);
            var patchData = PackageVersionAndDate(update.Packages.Patch);

            writer.WriteLine(
                $"{update.SelectedId},{packageSource}," +
                $"{occurences},{update.CountCurrentVersions()},{lowest},{highest}," +
                $"{majorData},{minorData},{patchData}");
        }

        private static string PackageVersionAndDate(PackageSearchMedatadata packageVersion)
        {
            if (packageVersion == null)
            {
                return ",";
            }

            var version = packageVersion.Identity.Version;
            var date = DateFormat.AsUtcIso8601(packageVersion.Published);
            return $"{version},{date}";
        }

        private StreamWriter MakeOutputStream(string name)
        {
            var fileName = name + "_nukeeeper_report.csv";

            _logger.Verbose($"writing report to file at '{fileName}'");

            var output = new FileStream(fileName, FileMode.OpenOrCreate);
            return new StreamWriter(output);
        }
    }
}
