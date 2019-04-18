using System.Collections.Generic;
using NuKeeper.Abstractions.Output;
using NuKeeper.Abstractions.RepositoryInspection;

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
