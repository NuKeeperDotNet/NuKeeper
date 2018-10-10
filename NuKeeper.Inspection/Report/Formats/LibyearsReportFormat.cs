using System.Collections.Generic;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report.Formats
{
    public class LibYearsReportFormat : IReportFormat
    {
        private readonly IReportWriter _writer;

        public LibYearsReportFormat(IReportWriter writer)
        {
            _writer = writer;
        }

        public void Write(string name, IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var totalAge = Age.Sum(updates);
            _writer.WriteLine(Age.AsLibYears(totalAge));
        }
    }
}
