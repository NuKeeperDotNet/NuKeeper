using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<PackageUpdateSet>> CanMakeBranchFor(ForkData pushFork, IEnumerable<PackageUpdateSet> packageUpdateSets)
        {
            var results = await packageUpdateSets
                .WhereAsync(async p => await CanMakeBranchFor(pushFork, p));

            return results.ToList();
        }

        private async Task<bool> CanMakeBranchFor(ForkData pushFork, PackageUpdateSet packageUpdateSet)
        {
            var branchName = BranchNamer.MakeName(packageUpdateSet);
            var githubBranch = await _github.GetRepositoryBranch(pushFork.Owner, pushFork.Name, branchName);
            return (githubBranch == null);
        }
    }
}
