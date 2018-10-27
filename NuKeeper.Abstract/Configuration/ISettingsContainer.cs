using NuKeeper.Update.Selection;

namespace NuKeeper.Abstract.Configuration
{
    public interface ISettingsContainer
    {
        SourceControlServerSettings SourceControlServerSettings { get; set; }

        IAuthSettings AuthSettings { get; set; }

        UserSettings UserSettings { get; set; }

        IFilterSettings PackageFilters { get; set; }
    }
}