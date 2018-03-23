using NuKeeper.Git;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public interface IExistingBranchFilter
    {
        bool HasExistingBranch(IGitDriver git, PackageUpdateSet packageUpdateSet);
    }
}
