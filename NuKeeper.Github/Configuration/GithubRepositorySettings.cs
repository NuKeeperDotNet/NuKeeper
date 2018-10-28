using System;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Github.Configuration
{
    public class GithubRepositorySettings
    {
        public GithubRepositorySettings(IRepositorySettings repository)
        {
            GithubUri = repository.Uri;
            RepositoryOwner = repository.Owner;
            RepositoryName = repository.RepositoryName;
        }

        public Uri GithubUri { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }
    }
}
