using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Inspection.Sort;
using NuKeeper.Update.Selection;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public class PackageUpdateSelection : IPackageUpdateSelection
    {
        private readonly INuKeeperLogger _logger;
        private readonly IExistingBranchFilter _existingBranchFilter;
        private readonly IPackageUpdateSetSort _sort;
        private readonly IUpdateSelection _updateSelection;


        public PackageUpdateSelection(
            IExistingBranchFilter existingBranchFilter,
            IPackageUpdateSetSort sort,
            IUpdateSelection updateSelection,
            INuKeeperLogger logger)
        {
            _logger = logger;
            _existingBranchFilter = existingBranchFilter;
            _sort = sort;
            _updateSelection = updateSelection;
        }

        public async Task<IReadOnlyCollection<PackageUpdateSet>> SelectTargets(
            ForkData pushFork,
            IReadOnlyCollection<PackageUpdateSet> potentialUpdates,
            FilterSettings filterSettings,
            BranchSettings branchSettings)
        {
            var sorted = _sort.Sort(potentialUpdates)
                .ToList();

            var filtered = await _updateSelection.Filter(
                sorted, filterSettings,
                p => _existingBranchFilter.CanMakeBranchFor(p, pushFork, branchSettings.BranchNamePrefix));

            foreach (var updateSet in filtered)
            {
                _logger.Normal($"Selected package update of {updateSet.SelectedId} to {updateSet.SelectedVersion}");
            }

            return filtered;
        }
    }
}
