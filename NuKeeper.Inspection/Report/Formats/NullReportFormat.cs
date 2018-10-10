using System.Collections.Generic;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report.Formats
{
    public class NullReportFormat : IReportFormat
    {
        public NullReportFormat(IReportWriter writer)
        {
        }

        public void Write(string name, IReadOnlyCollection<PackageUpdateSet> updates)
        {
        }
    }
}
