using System;
using NuKeeper.Abstract;

namespace NuKeeper.Github.Mappings
{
    public class GithubRepository : IRepository
    {
        private readonly Octokit.Repository _repository;

        public GithubRepository(Octokit.Repository repository)
        {
            _repository = repository;
        }

        public IRepository Parent => new GithubRepository(_repository.Parent);
            
        public RepositoryPermissions Permissions => new RepositoryPermissions(_repository.Permissions.Admin, _repository.Permissions.Push, _repository.Permissions.Pull);

        public Uri CloneUrl => new Uri(_repository.CloneUrl);

        public Uri HtmlUrl => new Uri(_repository.HtmlUrl);

        public User Owner => new User(_repository.Owner.Login, _repository.Owner.Name, _repository.Owner.Email);

        public string Name => _repository.Name;

        public bool Archived => _repository.Archived;

        public bool Fork => _repository.Fork;
    }
}
