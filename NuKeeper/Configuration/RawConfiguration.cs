using System;
using EasyConfig;
using EasyConfig.Attributes;

namespace NuKeeper.Configuration
{
    public class RawConfiguration
    {
        [CommandLine("mode", "m"), Required]
        public string Mode;

        [Environment("NuKeeper_github_token"), Required, SensitiveInformation]
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

        [JsonConfig("log_level"), Default("Info")]
        [OverriddenBy(ConfigurationSources.CommandLine, "log")]
        public string LogLevel;

        [JsonConfig("nuget_sources"), Default("https://api.nuget.org/v3/index.json")]
        [OverriddenBy(ConfigurationSources.CommandLine, "sources")]
        public string NuGetSources;

        [CommandLine("include", "i")]
        public string Include;

        [CommandLine("exclude", "e")]
        public string Exclude;

        [JsonConfig("allowed_version_change"), Default("Major")]
        [OverriddenBy(ConfigurationSources.CommandLine, "change")]
        public string AllowedChange;

    }
}
