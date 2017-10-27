using System.Text.RegularExpressions;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;

namespace NuKeeper.Configuration
{
    public class UserPreferences
    {
        public string[] NuGetSources { get; set; }

        public Regex PackageIncludes { get; set; }

        public Regex PackageExcludes { get; set; }

        public int MaxPullRequestsPerRepository { get; set; }

        public VersionChange AllowedChange { get; set; }

        public LogLevel LogLevel { get; set; }
    }
}