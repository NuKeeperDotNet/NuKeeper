using System;

namespace NuKeeper.Abstract.Configuration
{
    public class RepositorySettings : IRepositorySettings
    {
        public RepositorySettings(IRepository repository)
        {
            Uri = repository.HtmlUrl;
            Name = repository.Name;
            Owner = repository.Owner.Name;
            RepositoryName = repository.Name;
        }

        public RepositorySettings()
        {
        }

        public Uri Uri { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string RepositoryName { get; set; }
    }
}
