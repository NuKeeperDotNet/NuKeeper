using System.Collections.Generic;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine.Report
{
    public interface IAvailableUpdatesReporter
    {
        void Report(string name, List<PackageUpdateSet> updates);
    }
}