using NuKeeper.Inspection.Sources;

namespace NuKeeper.Inspection.NuGetApi
{
    public class PackageUpdateLookupSettings
    {
        public VersionChange AllowedChange { get; set; }

        public NuGetSources NuGetSources { get; set; }
    }
}
