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

        [JsonConfig("github_api_endpoint"), Required]
        [OverriddenBy(ConfigurationSources.CommandLine, "api")]
        public Uri GithubApiEndpoint;

        [JsonConfig("max_pull_requests_per_repository"), Required]
        [OverriddenBy(ConfigurationSources.CommandLine, "maxpr")]
        public int MaxPullRequestsPerRepository;
    }
}
