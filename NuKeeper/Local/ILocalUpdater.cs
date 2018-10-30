using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Local
{
    public interface ILocalUpdater
    {
        Task ApplyUpdates(
            IReadOnlyCollection<PackageUpdateSet> updates,
            IFolder workingFolder,
            NuGetSources sources,
            SettingsContainer settings);
    }
}
