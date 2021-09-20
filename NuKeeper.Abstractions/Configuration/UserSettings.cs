using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.Output;
using System.Collections.Generic;

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
        public string CommitMessageTemplate { get; set; }
        public string PullRequestTitleTemplate { get; set; }
        public string PullRequestBodyTemplate { get; set; }
        public IDictionary<string, object> Context { get; } = new Dictionary<string, object>();
    }
}
