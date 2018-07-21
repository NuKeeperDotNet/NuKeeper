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
        private readonly ICreate<IUpdateSelection> _updateSelectionCreator;
        private readonly INuKeeperLogger _logger;

        public PackageUpdateSelectionCreator(ICreate<IExistingBranchFilter> existingBranchFilterCreator,
            IPackageUpdateSetSort packageUpdateSetSort, ICreate<IUpdateSelection> updateSelectionCreator,
            INuKeeperLogger logger)
        {
            _existingBranchFilterCreator = existingBranchFilterCreator;
            _packageUpdateSetSort = packageUpdateSetSort;
            _updateSelectionCreator = updateSelectionCreator;
            _logger = logger;
        }

        public IPackageUpdateSelection Create(SettingsContainer settings)
        {
            return new PackageUpdateSelection(_existingBranchFilterCreator.Create(settings),
                _packageUpdateSetSort, _updateSelectionCreator.Create(settings), _logger);
        }
    }
}
