using System;
using NuKeeper.Abstractions.DTOs;

namespace NuKeeper.Configuration
{
    public class RepositorySettings
    {
        public RepositorySettings()
        {
        }

        public RepositorySettings(Repository repository)
        {
            Uri = repository.HtmlUrl;
            RepositoryOwner = repository.Owner.Login;
            RepositoryName = repository.Name;
        }

        public Uri Uri { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }
    }
}
