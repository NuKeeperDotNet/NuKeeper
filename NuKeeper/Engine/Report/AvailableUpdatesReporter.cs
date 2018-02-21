using NuKeeper.RepositoryInspection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuKeeper.Logging;

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
                WriteHeading(writer);
                _logger.Verbose($"writing {updates.Count} lines to report");

                foreach (var update in updates)
                {
                    WriteLine(writer, update);
                }

                writer.Close();
            }
        }

        private void WriteHeading(StreamWriter writer)
        {
            writer.WriteLine(
                "Package id,Package source," +
                "Usage count,Versions in use,Lowest version in use,Highest Version in use," +
                "Highest available,Highest published date");
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

            var major = update.Packages.Major;
            var highestVersion = major?.Identity.Version;
            var highestDate = DateFormat.AsUtcIso8601(major?.Published);

            writer.WriteLine(
                $"{update.SelectedId},{packageSource}," +
                $"{occurences},{update.CountCurrentVersions()},{lowest},{highest}," +
                $"{highestVersion},{highestDate}"
                );
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
