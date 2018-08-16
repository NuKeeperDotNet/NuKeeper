using NuKeeper.Update.Selection;

namespace NuKeeper.Configuration
{
    public class SettingsContainer
    {
        public ModalSettings ModalSettings { get; set; }

        public GithubAuthSettings GithubAuthSettings { get; set; }

        public UserSettings UserSettings { get; set; }

        public FilterSettings PackageFilters { get; set; }
    }
}
