using System;
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

        public async Task<bool> CanMakeBranchFor(PackageUpdateSet packageUpdateSet,
            ForkData pushFork)
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
