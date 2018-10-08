using System.Collections.Generic;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report
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

            WriteLibYears(updates);
            _writer.Close();
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
