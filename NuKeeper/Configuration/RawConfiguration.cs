using System;
using EasyConfig;
using EasyConfig.Attributes;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;

namespace NuKeeper.Configuration
{
    public class RawConfiguration
    {
        [CommandLine("mode", "m"), Required, Default("inspect")]
        public string Mode;

        [Environment("NuKeeper_github_token"), SensitiveInformation]
        [OverriddenBy(ConfigurationSources.CommandLine, "t")]
        public string GithubToken;

        [CommandLine("github_repository_uri", "repo")]
        public Uri GithubRepositoryUri;

        [CommandLine("github_organisation_name", "org")]
        public string GithubOrganisationName;

        [JsonConfig("github_api_endpoint"), Default("https://api.github.com")]
        [OverriddenBy(ConfigurationSources.CommandLine, "api")]
        public Uri GithubApiEndpoint;

        [JsonConfig("max_pull_requests_per_repository"), Default(3)]
        [OverriddenBy(ConfigurationSources.CommandLine, "maxpr")]
        public int MaxPullRequestsPerRepository;

        [JsonConfig("max_repositories_changed"), Default(10)]
        [OverriddenBy(ConfigurationSources.CommandLine, "maxrepo")]
        public int MaxRepositoriesChanged;

        [JsonConfig("log_level"), Default("Info")]
        [OverriddenBy(ConfigurationSources.CommandLine, "log")]
        public LogLevel LogLevel;

        [JsonConfig("nuget_sources")]
        [OverriddenBy(ConfigurationSources.CommandLine, "sources")]
        public string NuGetSources;

        [CommandLine("include", "i")]
        public string Include;

        [CommandLine("exclude", "e")]
        public string Exclude;

        [JsonConfig("allowed_version_change"), Default("Major")]
        [OverriddenBy(ConfigurationSources.CommandLine, "change")]
        public VersionChange AllowedChange;

        [JsonConfig("min_package_age"), Default("7d")]
        [OverriddenBy(ConfigurationSources.CommandLine, "age")]
        public string MinPackageAge;

        [JsonConfig("fork_mode"), Default("PreferFork")]
        [OverriddenBy(ConfigurationSources.CommandLine, "fork")]
        public ForkMode ForkMode;

        [JsonConfig("report_mode"), Default("Off")]
        [OverriddenBy(ConfigurationSources.CommandLine, "report")]
        public ReportMode ReportMode;

        [CommandLine("dir")]
        public string Dir { get; set; }

        [JsonConfig("github_labels"), Default("nukeeper")]
        [OverriddenBy(ConfigurationSources.CommandLine, "labels")]
        public string Labels;
    }
}
