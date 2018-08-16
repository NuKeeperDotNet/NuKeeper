using System;
using System.Text.RegularExpressions;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Configuration
{
    public class UserSettings
    {
        public NuGetSources NuGetSources { get; set; }

        public int MaxRepositoriesChanged { get; set; }

        public VersionChange AllowedChange { get; set; }

        public ForkMode ForkMode { get; set; }

        public ReportMode ReportMode { get; set; }

        public string Directory { get; set; }
    }
}
