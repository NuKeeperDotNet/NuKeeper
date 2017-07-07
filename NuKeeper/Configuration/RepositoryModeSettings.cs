using System;
using NuKeeper.Github;

namespace NuKeeper.Configuration
{
    public class RepositoryModeSettings
    {
        public RepositoryModeSettings()
        {
        }

        public RepositoryModeSettings(GithubRepository repository, Uri githubApi, string githubToken)
        {
            GithubApiBase = githubApi;
            GithubToken = githubToken;
            GithubUri = new Uri(repository.HtmlUrl);
            RepositoryOwner = repository.Owner.Login;
            RepositoryName = repository.Name;
        }

        public Uri GithubUri { get; set; }

        public Uri GithubApiBase { get; set; }
        public string GithubToken { get; set; }

        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }
    }
}