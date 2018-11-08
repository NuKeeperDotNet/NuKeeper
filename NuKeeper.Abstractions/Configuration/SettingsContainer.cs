namespace NuKeeper.Abstractions.Configuration
{
    public class SettingsContainer
    {
        public SourceControlServerSettings SourceControlServerSettings { get; set; }

        public UserSettings UserSettings { get; set; }

        public FilterSettings PackageFilters { get; set; }
    }
}
