using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using System;
using System.Threading.Tasks;

namespace NuKeeper.Engine.Packages
{
    public class ExistingBranchFilter : IExistingBranchFilter
    {
        private readonly ICollaborationPlatform _collaborationPlatform;
        private readonly INuKeeperLogger _logger;

        public ExistingBranchFilter(ICollaborationPlatform collaborationPlatform, INuKeeperLogger logger)
        {
            _collaborationPlatform = collaborationPlatform;
            _logger = logger;
        }

        public async Task<bool> CanMakeBranchFor(PackageUpdateSet packageUpdateSet,
            ForkData pushFork)
        {
            try
            {
                var branchName = BranchNamer.MakeSinglePackageName(packageUpdateSet);
                return await _collaborationPlatform.RepositoryBranchExists(pushFork.Owner, pushFork.Name, branchName);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed on existing branch check at {pushFork.Owner}/{pushFork.Name}", ex);
                return false;
            }
        }
    }
}
