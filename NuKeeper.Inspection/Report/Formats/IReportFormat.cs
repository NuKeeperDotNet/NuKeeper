using System.Collections.Generic;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Inspection.Report.Formats
{
    public interface IReportFormat
    {
        void Write(
            string name,
            IReadOnlyCollection<PackageUpdateSet> updates);
    }
}
