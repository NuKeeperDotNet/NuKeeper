using System;

namespace NuKeeper.Abstract.Configuration
{
    public class RepositorySettings : IRepositorySettings
    {
        private readonly IRepository _repository;

        public RepositorySettings(IRepository repository)
        {
            _repository = repository;
        }

        public RepositorySettings()
        {
        }

        public Uri Uri { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string RepositoryName { get; }
    }
}
