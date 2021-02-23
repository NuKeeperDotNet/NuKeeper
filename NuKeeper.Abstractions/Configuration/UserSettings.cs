using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.Output;

namespace NuKeeper.Abstractions.Configuration
{
    public class UserSettings
    {
        public NuGetSources NuGetSources { get; set; }

        public int MaxRepositoriesChanged { get; set; }
        public int MaxOpenPullRequests { get; set; }

        public bool ConsolidateUpdatesInSinglePullRequest { get; set; }

        public VersionChange AllowedChange { get; set; }

        public UsePrerelease UsePrerelease { get; set; }


        public OutputFormat OutputFormat { get; set; }
        public OutputDestination OutputDestination { get; set; }
        public string OutputFileName { get; set; }

        public string Directory { get; set; }

        public string GitPath { get; set; }
    }
}
