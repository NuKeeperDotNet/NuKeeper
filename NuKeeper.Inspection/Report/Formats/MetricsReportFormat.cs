using System.Collections.Generic;
using System.Linq;
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
            _writer.WriteLine($"PackageUpdates: {updates.Count}");

            // todo: count major, minor and patch updates

            WriteProjectCount(updates);
            WriteLibYears(updates);
            _writer.Close();
        }

        private void WriteProjectCount(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var projectCount = updates
                .SelectMany(p => p.CurrentPackages)
                .Select(c => c.Path.FullName)
                .Distinct()
                .Count();

            _writer.WriteLine($"Projects: {projectCount}");
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
