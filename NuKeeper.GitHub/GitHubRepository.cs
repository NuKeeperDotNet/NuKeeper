using NuKeeper.Abstractions.CollaborationModels;

namespace NuKeeper.GitHub
{
    internal class GitHubRepository : Repository
    {
        public GitHubRepository(Octokit.Repository repository)
        : base(
            repository.Name,
            repository.Archived,
            repository.Permissions != null ?
                new UserPermissions(repository.Permissions.Admin, repository.Permissions.Push, repository.Permissions.Pull) : null,
            GithubUriHelpers.Normalise(repository.CloneUrl),
            new User(repository.Owner.Login, repository.Owner.Name, repository.Owner.Email),
            repository.Fork,
            repository.Parent != null ?
                new GitHubRepository(repository.Parent) : null
            )
        {
        }
    }
}
