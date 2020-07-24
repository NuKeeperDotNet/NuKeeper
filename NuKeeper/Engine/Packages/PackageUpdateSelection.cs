using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using NuKeeper.Update.Selection;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Engine.Packages
{
    public class PackageUpdateSelection : IPackageUpdateSelection
    {
        private readonly INuKeeperLogger _logger;
        private readonly IPackageUpdateSetSort _sort;
        private readonly IUpdateSelection _updateSelection;

        public PackageUpdateSelection(
            IPackageUpdateSetSort sort,
            IUpdateSelection updateSelection,
            INuKeeperLogger logger)
        {
            _logger = logger;
            _sort = sort;
            _updateSelection = updateSelection;
        }

        public IReadOnlyCollection<PackageUpdateSet> SelectTargets(
            ForkData pushFork,
            IReadOnlyCollection<PackageUpdateSet> potentialUpdates,
            FilterSettings filterSettings)
        {
            var sorted = _sort.Sort(potentialUpdates)
                .ToList();

            var filtered = _updateSelection.Filter(sorted, filterSettings);

            foreach (var updateSet in filtered)
            {
                _logger.Normal($"Selected package update of {updateSet.SelectedId} to {updateSet.SelectedVersion}");
            }

            return filtered;
        }
    }
}
