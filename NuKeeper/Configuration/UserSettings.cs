using System;
using System.Text.RegularExpressions;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NuKeeper.Update;

namespace NuKeeper.Configuration
{
    public class UserSettings
    {
        public NuGetSources NuGetSources { get; set; }

        public Regex PackageIncludes { get; set; }

        public Regex PackageExcludes { get; set; }

        public int MaxPullRequestsPerRepository { get; set; }

        public TimeSpan MinimumPackageAge { get; set; }

        public VersionChange AllowedChange { get; set; }

        public ForkMode ForkMode { get; set; }

        public LogLevel LogLevel { get; set; }

        public ReportMode ReportMode { get; set; }

        public string Directory { get; set; }
    }
}
