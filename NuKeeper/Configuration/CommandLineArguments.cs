using System;
using EasyConfig.Attributes;

namespace NuKeeper.Configuration
{
    public class CommandLineArguments
    {
        [CommandLine("mode", "m"), Required]
        public string Mode;

        [CommandLine("github_user", "u"), Required, SensitiveInformation]
        public string GithubUser;

        [CommandLine("github_token", "t"), Required, SensitiveInformation]
        public string GithubToken;

        [CommandLine("github_repository_uri", "repo")]
        public Uri GithubRepositoryUri;

        [CommandLine("github_organisation_name", "org")]
        public string GithubOrganisationName;

        [CommandLine("github_api_endpoint", "api"), Default("https://api.github.com")]
        public Uri GithubApiEndpoint;

        [CommandLine("max_pull_requests_per_repository", "maxpr"), Default(3)]
        public int MaxPullRequestsPerRepository;
    }
}
