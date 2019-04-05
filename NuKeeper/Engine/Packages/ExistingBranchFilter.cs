using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using System;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;

namespace NuKeeper.Engine.Packages
{
    public class ExistingBranchFilter : IExistingBranchFilter
    {
        private readonly ICollaborationFactory _collaborationFactory;
        private readonly INuKeeperLogger _logger;

        public ExistingBranchFilter(ICollaborationFactory collaborationFactory, INuKeeperLogger logger)
        {
            _collaborationFactory = collaborationFactory;
            _logger = logger;
        }

        public async Task<bool> CanMakeBranchFor(PackageUpdateSet packageUpdateSet,
            ForkData pushFork)
        {
            try
            {
                var branchName = BranchNamer.MakeSinglePackageName(packageUpdateSet);
                var branchExists = await _collaborationFactory.CollaborationPlatform.RepositoryBranchExists(pushFork.Owner, pushFork.Name, branchName);
                return !branchExists;
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _logger.Error($"Failed on existing branch check at {pushFork.Owner}/{pushFork.Name}", ex);
                return false;
            }
        }
    }
}
