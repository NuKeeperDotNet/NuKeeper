using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Local;
using NuKeeper.Update;
using NuKeeper.Update.Selection;

namespace NuKeeper.Creators
{
    public class LocalUpdaterCreator : ICreate<ILocalUpdater>
    {
        private readonly INuKeeperLogger _logger;
        private readonly IUpdateRunner _updateRunner;
        private readonly ICreate<IUpdateSelection> _updateSelectionCreator;

        public LocalUpdaterCreator(INuKeeperLogger logger, IUpdateRunner updateRunner, ICreate<IUpdateSelection> updateSelectionCreator)
        {
            _logger = logger;
            _updateRunner = updateRunner;
            _updateSelectionCreator = updateSelectionCreator;
        }

        public ILocalUpdater Create(SettingsContainer settings)
        {
            return new LocalUpdater(_updateSelectionCreator.Create(settings), _updateRunner, _logger);
        }
    }
}
