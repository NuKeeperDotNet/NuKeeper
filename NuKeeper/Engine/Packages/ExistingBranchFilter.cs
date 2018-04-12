using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Github;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public class ExistingBranchFilter : IExistingBranchFilter
    {
        private readonly IGithub _github;
        private readonly INuKeeperLogger _logger;

        public ExistingBranchFilter(IGithub github, INuKeeperLogger logger)
        {
            _github = github;
            _logger = logger;
        }

        public async Task<IEnumerable<PackageUpdateSet>> CanMakeBranchFor(ForkData pushFork, IEnumerable<PackageUpdateSet> packageUpdateSets)
        {
            var results = await packageUpdateSets
                .WhereAsync(async p => await CanMakeBranchFor(pushFork, p));

            return results.ToList();
        }

        private async Task<bool> CanMakeBranchFor(ForkData pushFork, PackageUpdateSet packageUpdateSet)
        {
            try
            {
                var branchName = BranchNamer.MakeName(packageUpdateSet);
                var githubBranch = await _github.GetRepositoryBranch(pushFork.Owner, pushFork.Name, branchName);
                return (githubBranch == null);
            }
            catch(Exception ex)
            {
                _logger.Error($"Failed on existing branch check at {pushFork.Owner}/{pushFork.Name}", ex);
                return false;
            }
        }
    }
}
