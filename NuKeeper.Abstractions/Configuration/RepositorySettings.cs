using System;

namespace NuKeeper.Abstractions.Configuration
{
    public class RepositorySettings : IRepositorySettings
    {
        public RepositorySettings(IRepository repository)
            :this(repository.Owner.Name, repository.Name, repository.HtmlUrl)
        {
        }

        public RepositorySettings(string owner, string repositoryName, Uri uri = null)
        {
            Uri = uri;
            Owner = owner;
            RepositoryName = repositoryName;
        }


        public Uri Uri { get; }
        public string Owner { get; }
        public string RepositoryName { get; }
    }
}
