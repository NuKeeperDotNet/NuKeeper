using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Update.Selection;

namespace NuKeeper.Creators
{
    public class UpdateSelectionCreator : ICreate<IUpdateSelection>
    {
        private readonly INuKeeperLogger _logger;

        public UpdateSelectionCreator(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public IUpdateSelection Create(SettingsContainer settings)
        {
            return new UpdateSelection(MakeFilterSettings(settings.UserSettings), _logger);
        }

        private static FilterSettings MakeFilterSettings(UserSettings settings)
        {
            return new FilterSettings
            {
                Excludes = settings.PackageExcludes,
                Includes = settings.PackageIncludes,
                MaxPackageUpdates = settings.MaxPackageUpdates,
                MinimumAge = settings.MinimumPackageAge
            };
        }
    }
}
