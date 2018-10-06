using System.Collections.Generic;
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
                _writer.WriteLine(Description.ForUpdateSet(update));
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
    }
}
