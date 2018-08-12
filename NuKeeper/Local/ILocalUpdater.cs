using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Local
{
    public interface ILocalUpdater
    {
        Task ApplyUpdates(
            IReadOnlyCollection<PackageUpdateSet> updates,
            NuGetSources sources);
    }
}
