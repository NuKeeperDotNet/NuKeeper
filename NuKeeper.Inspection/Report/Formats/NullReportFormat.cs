using System.Collections.Generic;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.Report.Formats
{
    public class NullReportFormat : IReportFormat
    {
        public void Write(string name, IReadOnlyCollection<PackageUpdateSet> updates)
        {
        }
    }
}
