using System;
using System.Threading.Tasks;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Logging;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public class ExistingBranchFilter : IExistingBranchFilter
    {
        private readonly IGitHub _gitHub;
        private readonly INuKeeperLogger _logger;

        public ExistingBranchFilter(IGitHub gitHub, INuKeeperLogger logger)
        {
            _gitHub = gitHub;
            _logger = logger;
        }

        public async Task<bool> CanMakeBranchFor(PackageUpdateSet packageUpdateSet,
            ForkData pushFork)
        {
            try
            {
                var branchName = BranchNamer.MakeSinglePackageName(packageUpdateSet);
                return await _gitHub.RepositoryBranchExists(pushFork.Owner, pushFork.Name, branchName);
            }
            catch(Exception ex)
            {
                _logger.Error($"Failed on existing branch check at {pushFork.Owner}/{pushFork.Name}", ex);
                return false;
            }
        }
    }
}
