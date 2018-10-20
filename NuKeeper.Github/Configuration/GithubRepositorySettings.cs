using System;
using NuKeeper.Abstract;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Configuration;
using Octokit;

namespace NuKeeper.Github.Configuration
{
    public class GithubRepositorySettings
    {
        public GithubRepositorySettings()
        {
        }

        public GithubRepositorySettings(IRepositorySettings repository)
        {
            GithubUri = repository.Uri;
            RepositoryOwner = repository.Owner;
            RepositoryName = repository.Name;
        }

        public Uri GithubUri { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }
    }
}
