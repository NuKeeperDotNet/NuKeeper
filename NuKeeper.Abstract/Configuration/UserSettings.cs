using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Abstract.Configuration
{
    public abstract class UserSettings
    {
        public NuGetSources NuGetSources { get; set; }

        public int MaxRepositoriesChanged { get; set; }

        public bool ConsolidateUpdatesInSinglePullRequest { get; set; }

        public VersionChange AllowedChange { get; set; }

        public ForkMode ForkMode { get; set; }

        public OutputFormat OutputFormat { get; set; }

        public OutputDestination OutputDestination { get; set; }

        public string OutputFileName { get; set; }

        public string Directory { get; set; }
    }
}
