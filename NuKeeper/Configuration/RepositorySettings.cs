using System;
using NuKeeper.GitHub;

namespace NuKeeper.Configuration
{
    public class RepositorySettings
    {
        public RepositorySettings()
        {
        }

        public RepositorySettings(IRepository repository)
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
