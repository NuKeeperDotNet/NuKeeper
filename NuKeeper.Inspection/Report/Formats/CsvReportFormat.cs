using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report.Formats
{
    public class CsvReportFormat: IReportFormat
    {
        private readonly IReportWriter _writer;

        public CsvReportFormat(IReportWriter writer)
        {
            _writer = writer;
        }

        public void Write(string name, IReadOnlyCollection<PackageUpdateSet> updates)
        {
            WriteHeading();

            foreach (var update in updates)
            {
                WriteLine(update);
            }
        }

        private void WriteHeading()
        {
            _writer.WriteLine(
                "Package id,Package source," +
                "Usage count,Versions in use,Lowest version in use,Highest Version in use," +
                "Major version update,Major published date," +
                "Minor version update,Minor published date," +
                "Patch version update,Patch published date"
                );
        }

        private void WriteLine(PackageUpdateSet update)
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

            _writer.WriteLine(
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
