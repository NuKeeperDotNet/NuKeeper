using System.Threading.Tasks;
using NuKeeper.Git;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Engine.Packages
{
    public interface IPackageUpdater
    {
        Task<bool> MakeUpdatePullRequest(
            IGitDriver git,
            PackageUpdateSet updateSet,
            NuGetSources sources,
            RepositoryData repository);
    }
}
