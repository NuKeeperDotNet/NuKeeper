using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public interface IPackageUpdater
    {
        Task<int> MakeUpdatePullRequests(
            IGitDriver git,
            RepositoryData repository,
            IReadOnlyCollection<PackageUpdateSet> updates,
            NuGetSources sources,
            SettingsContainer settings);
    }
}
