using NuKeeper.Abstractions.DTOs;
using System;
using Repository = NuKeeper.Abstractions.DTOs.Repository;

namespace NuKeeper.GitHub
{
    public class GitHubRepository : Repository
    {
        public GitHubRepository(Octokit.Repository repository)
        : base(
            repository.Name,
            repository.Archived,
            repository.Permissions != null ?
                new UserPermissions(repository.Permissions.Admin, repository.Permissions.Push, repository.Permissions.Pull) : null,
            new Uri(repository.HtmlUrl),
            new Uri(repository.CloneUrl),
            new User(repository.Owner.Login, repository.Owner.Name, repository.Owner.Email),
            repository.Fork,
            repository.Parent != null ?
                new GitHubRepository(repository.Parent) : null
            )
        {
        }
    }
}
