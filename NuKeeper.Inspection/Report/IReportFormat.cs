using System.Collections.Generic;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report
{
    public interface IReportFormat
    {
        void Write(
            string name,
            IReadOnlyCollection<PackageUpdateSet> updates);
    }
}
