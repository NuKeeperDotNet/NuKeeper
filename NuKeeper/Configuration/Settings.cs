using System;

namespace NuKeeper.Configuration
{
    public class Settings
    {
        public const string RepositoryMode = "repository";
        public const string OrganisationMode = "organisation";

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

        public string Mode { get; }
    }
}