using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Abstract.Local
{
    public interface ILocalUpdater
    {
        Task ApplyUpdates(
            IReadOnlyCollection<PackageUpdateSet> updates,
            IFolder workingFolder,
            NuGetSources sources,
            ISettingsContainer settings);
    }
}
