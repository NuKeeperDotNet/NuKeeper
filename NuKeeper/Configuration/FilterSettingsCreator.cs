using NuKeeper.Configuration;
using NuKeeper.Update.Selection;

namespace NuKeeper.Creators
{
    public class FilterSettingsCreator
    {
        public static FilterSettings MakeFilterSettings(UserSettings settings)
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
