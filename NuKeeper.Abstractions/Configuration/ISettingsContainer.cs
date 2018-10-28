using NuKeeper.Update.Selection;

namespace NuKeeper.Abstractions.Configuration
{
    public interface ISettingsContainer
    {
        SourceControlServerSettings SourceControlServerSettings { get; set; }

        IAuthSettings AuthSettings { get; set; }

        UserSettings UserSettings { get; set; }

        IFilterSettings PackageFilters { get; set; }
    }
}
