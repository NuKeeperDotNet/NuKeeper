using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public class ExistingBranchFilter : IExistingBranchFilter
    {
        private readonly IGithub _github;

        public ExistingBranchFilter(IGithub github)
        {
            _github = github;
        }

        public bool HasExistingBranch(IGitDriver git, PackageUpdateSet packageUpdateSet)
        {
            var qualifiedBranchName = "origin/" + BranchNamer.MakeName(packageUpdateSet);
            return git.BranchExists(qualifiedBranchName);
        }

    }
}
