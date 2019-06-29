using NuKeeper.Abstractions.Inspections.Files;

namespace NuKeeper.Abstractions.Configuration
{
    public interface ISettingsContainer
    {
        SourceControlServerSettings SourceControlServerSettings { get; set; }

        UserSettings UserSettings { get; set; }

        FilterSettings PackageFilters { get; set; }

        BranchSettings BranchSettings { get; set; }

        IFolder WorkingFolder { get; set; }
    }
}
