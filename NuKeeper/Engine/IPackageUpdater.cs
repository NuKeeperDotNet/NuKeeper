using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Git;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public interface IPackageUpdater
    {
        Task UpdatePackageInProjects(
            IGitDriver git,
            PackageUpdateSet updateSet,
            RepositorySettings settings,
            string defaultBranch);
    }
}