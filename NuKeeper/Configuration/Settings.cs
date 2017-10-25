using System;
using System.Text.RegularExpressions;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;

namespace NuKeeper.Configuration
{
    public class Settings
    {
        public const string RepoMode = "repo";
        public const string RepositoryMode = "repository";

        public const string OrganisationMode = "organisation";
        public const string OrgMode = "org";

        public Settings(RepositoryModeSettings repositoryModeSettings)
        {
            Repository = repositoryModeSettings;
            Mode = RepositoryMode;
        }

        public Settings(OrganisationModeSettings organisationModeSettings)
        {
            Organisation = organisationModeSettings;
            Mode = OrganisationMode;
        }

        public Settings(RepositoryModeSettings repository, OrganisationModeSettings organisation)
        {
            Repository = repository;
            Organisation = organisation;
        }

        public RepositoryModeSettings Repository { get; }
        public OrganisationModeSettings Organisation { get; }

        public string GithubToken => Repository?.GithubToken ?? Organisation?.GithubToken;
        public Uri GithubApiBase => Repository?.GithubApiBase ?? Organisation?.GithubApiBase;

        public int MaxPullRequestsPerRepository => Repository?.MaxPullRequestsPerRepository ?? Organisation?.MaxPullRequestsPerRepository ?? 0;

        public VersionChange AllowedChange { get; set; }

        public string Mode { get; }

        public LogLevel LogLevel { get; set; }

        public string[] NuGetSources { get; set; }

        public Regex PackageIncludes { get; set; }

        public Regex PackageExcludes { get; set; }
    }
}
