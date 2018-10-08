using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report.Formats
{
    public class MetricsReportFormat : IReportFormat
    {
        private readonly IReportWriter _writer;

        public MetricsReportFormat(IReportWriter writer)
        {
            _writer = writer;
        }

        public void Write(string name, IReadOnlyCollection<PackageUpdateSet> updates)
        {
            _writer.WriteLine($"Package updates: {updates.Count}");
            WriteMajorMinorPatchCount(updates);
            WriteProjectCount(updates);
            WriteLibYears(updates);
            _writer.Close();
        }

        private void WriteMajorMinorPatchCount(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var majors = 0;
            var minors = 0;
            var patches = 0;
            foreach (var update in updates)
            {
                var selectedUpdate = update.SelectedVersion;
                var minCurrent = MinCurrentVersion(update);

                if (selectedUpdate.Major > minCurrent.Major)
                {
                    majors++;
                }
                else if (selectedUpdate.Minor > minCurrent.Minor)
                {
                    minors++;
                }
                else if (selectedUpdate.Patch > minCurrent.Patch)
                {
                    patches++;
                }
            }

            _writer.WriteLine($"Major version updates: {majors}");
            _writer.WriteLine($"Minor version updates: {minors}");
            _writer.WriteLine($"Patch version updates: {patches}");
        }

        private static NuGetVersion MinCurrentVersion(PackageUpdateSet updates)
        {
            return updates.CurrentPackages
                .Select(p => p.Version)
                .Min();
        }

        private void WriteProjectCount(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var projectCount = updates
                .SelectMany(p => p.CurrentPackages)
                .Select(c => c.Path.FullName)
                .Distinct()
                .Count();

            _writer.WriteLine($"Projects with updates: {projectCount}");
        }

        private void WriteLibYears(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var totalAge = Age.Sum(updates);
            var years = totalAge.TotalDays / 365;

            _writer.WriteLine($"LibDays: {totalAge:%d}");
            _writer.WriteLine($"LibYears: {years:0.0}");
        }
    }
}
