using System;
using Octokit;

namespace NuKeeper.Configuration
{
    public class RepositoryModeSettings
    {
        public RepositoryModeSettings()
        {
        }

        public RepositoryModeSettings(Repository repository, 
            Uri githubApi, string githubToken, int maxPullRequestsPerRepository)
        {
            GithubApiBase = githubApi;
            GithubToken = githubToken;
            GithubUri = new Uri(repository.HtmlUrl);
            RepositoryOwner = repository.Owner.Login;
            RepositoryName = repository.Name;
            MaxPullRequestsPerRepository = maxPullRequestsPerRepository;
        }

        public Uri GithubUri { get; set; }

        public Uri GithubApiBase { get; set; }
        public string GithubToken { get; set; }

        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }

        public int MaxPullRequestsPerRepository { get; set; }
    }
}