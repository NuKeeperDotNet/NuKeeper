using NuKeeper.Abstractions.Inspections.Files;

namespace NuKeeper.Abstractions.Configuration
{
    public class SettingsContainer
    {
        public SourceControlServerSettings SourceControlServerSettings { get; set; }

        public UserSettings UserSettings { get; set; }

        public FilterSettings PackageFilters { get; set; }

        public BranchSettings BranchSettings { get; set; }

        public IFolder WorkingFolder { get; set; }
    }
}
