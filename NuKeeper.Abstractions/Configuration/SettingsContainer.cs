using NuKeeper.Update.Selection;

namespace NuKeeper.Abstractions.Configuration
{
    public class SettingsContainer : ISettingsContainer
    {
        public SourceControlServerSettings SourceControlServerSettings { get; set; }

        public AuthSettings AuthSettings { get; set; }

        public UserSettings UserSettings { get; set; }

        public FilterSettings PackageFilters { get; set; }
    }
}
