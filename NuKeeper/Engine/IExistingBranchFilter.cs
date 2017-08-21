using NuKeeper.Git;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public interface IExistingBranchFilter
    {
        bool Exists(IGitDriver git, PackageUpdateSet packageUpdateSet);
    }
}