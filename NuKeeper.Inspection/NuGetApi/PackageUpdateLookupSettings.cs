using System.Collections.Generic;

namespace NuKeeper.Inspection.NuGetApi
{
    public class PackageUpdateLookupSettings
    {
        public VersionChange AllowedChange { get; set; }

        public List<string> NugetSources { get; set; }
    }
}
