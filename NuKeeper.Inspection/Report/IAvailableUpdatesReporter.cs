using System.Collections.Generic;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report
{
    public interface IAvailableUpdatesReporter
    {
        void Report(string name, List<PackageUpdateSet> updates);
    }
}
