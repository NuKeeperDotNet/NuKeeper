using NuKeeper.Abstractions.Configuration;
using NuKeeper.Update.Selection;

namespace NuKeeper.Configuration
{
    public class SettingsContainer
    {
        public SourceControlServerSettings SourceControlServerSettings { get; set; }

        public AuthSettings GithubAuthSettings { get; set; }

        public UserSettings UserSettings { get; set; }

        public FilterSettings PackageFilters { get; set; }
    }
}
