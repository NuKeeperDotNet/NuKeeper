using System;
using System.Threading.Tasks;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Abstractions.Engine.Packages
{
    public class ExistingBranchFilter : IExistingBranchFilter
    {
        private readonly IClient _client;
        private readonly INuKeeperLogger _logger;

        public ExistingBranchFilter(IClient gitHub, INuKeeperLogger logger)
        {
            _client = gitHub;
            _logger = logger;
        }

        public async Task<bool> CanMakeBranchFor(PackageUpdateSet packageUpdateSet,
            ForkData pushFork)
        {
            try
            {
                var branchName = BranchNamer.MakeSinglePackageName(packageUpdateSet);
                var githubBranch = await _client.GetRepositoryBranch(pushFork.Owner, pushFork.Name, branchName);
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
