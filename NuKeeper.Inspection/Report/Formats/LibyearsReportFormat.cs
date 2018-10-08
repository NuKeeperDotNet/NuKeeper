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
            var years = totalAge.TotalDays / 365;

            _writer.WriteLine(years.ToString("0.0"));

            _writer.Close();
        }


    }
}
