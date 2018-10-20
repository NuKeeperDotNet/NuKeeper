using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using NuKeeper.Update.Selection;

namespace NuKeeper.Abstract.Engine.Packages
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
            IForkData pushFork,
            IReadOnlyCollection<PackageUpdateSet> potentialUpdates,
            IFilterSettings settings)
        {
            var sorted = _sort.Sort(potentialUpdates)
                .ToList();

            var filtered = await _updateSelection.Filter(
                sorted, settings,
                p => _existingBranchFilter.CanMakeBranchFor(p, pushFork)).ConfigureAwait(false);

            foreach (var updateSet in filtered)
            {
                _logger.Normal($"Selected package update of {updateSet.SelectedId} to {updateSet.SelectedVersion}");
            }

            return filtered;
        }
    }
}
