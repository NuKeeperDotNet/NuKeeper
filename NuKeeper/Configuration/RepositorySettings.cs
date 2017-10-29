using System;
using Octokit;

namespace NuKeeper.Configuration
{
    public class RepositorySettings
    {
        public RepositorySettings()
        {            
        }

        public RepositorySettings(Repository repository)
        {
            GithubUri = new Uri(repository.HtmlUrl);
            RepositoryOwner = repository.Owner.Login;
            RepositoryName = repository.Name;
        }

        public Uri GithubUri { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }
    }
}