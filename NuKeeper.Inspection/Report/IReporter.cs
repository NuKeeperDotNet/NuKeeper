using System.Collections.Generic;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report
{
    public interface IReporter
    {
        void Report(
            OutputDestination destination,
            OutputFormat format,
            string reportName,
            string fileName,
            IReadOnlyCollection<PackageUpdateSet> updates);
    }
}
