using NuKeeper.Git;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public class ExistingBranchFilter : IExistingBranchFilter
    {
        public bool Exists(IGitDriver git, PackageUpdateSet packageUpdateSet)
        {
            var branchName = BranchNamer.MakeName(packageUpdateSet);
            return  git.BranchExists("origin/" + branchName);
        }
    }
}