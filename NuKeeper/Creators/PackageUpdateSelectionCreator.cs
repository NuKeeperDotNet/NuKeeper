using NuKeeper.Configuration;
using NuKeeper.Engine.Packages;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Sort;
using NuKeeper.Update.Selection;

namespace NuKeeper.Creators
{
    public class PackageUpdateSelectionCreator : ICreate<IPackageUpdateSelection>
    {
        private readonly ICreate<IExistingBranchFilter> _existingBranchFilterCreator;
        private readonly IPackageUpdateSetSort _packageUpdateSetSort;
        private readonly IUpdateSelection _updateSelection;
        private readonly INuKeeperLogger _logger;

        public PackageUpdateSelectionCreator(
            ICreate<IExistingBranchFilter> existingBranchFilterCreator,
            IPackageUpdateSetSort packageUpdateSetSort,
            IUpdateSelection updateSelection,
            INuKeeperLogger logger)
        {
            _existingBranchFilterCreator = existingBranchFilterCreator;
            _packageUpdateSetSort = packageUpdateSetSort;
            _updateSelection = updateSelection;
            _logger = logger;
        }

        public IPackageUpdateSelection Create(SettingsContainer settings)
        {
            return new PackageUpdateSelection(_existingBranchFilterCreator.Create(settings),
                _packageUpdateSetSort, _updateSelection, _logger);
        }
    }
}
