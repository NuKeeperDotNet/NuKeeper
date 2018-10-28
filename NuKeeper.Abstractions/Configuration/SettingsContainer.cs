using NuKeeper.Update.Selection;

namespace NuKeeper.Abstractions.Configuration
{
    public class SettingsContainer : ISettingsContainer
    {
        public SourceControlServerSettings SourceControlServerSettings { get; set; }

        public IAuthSettings AuthSettings { get; set; }

        public UserSettings UserSettings { get; set; }

        public IFilterSettings PackageFilters { get; set; }
    }
}
