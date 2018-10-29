using NuKeeper.Update.Selection;

namespace NuKeeper.Abstractions.Configuration
{
    public interface ISettingsContainer
    {
        SourceControlServerSettings SourceControlServerSettings { get; set; }

        AuthSettings AuthSettings { get; set; }

        UserSettings UserSettings { get; set; }

        FilterSettings PackageFilters { get; set; }
    }
}
