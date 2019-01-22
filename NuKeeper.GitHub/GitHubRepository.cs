using System;
using NuKeeper.Abstractions.CollaborationModels;

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
            GithubUri(repository.CloneUrl),
            new User(repository.Owner.Login, repository.Owner.Name, repository.Owner.Email),
            repository.Fork,
            repository.Parent != null ?
                new GitHubRepository(repository.Parent) : null
            )
        {
        }

        private static Uri GithubUri(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (value.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(0, value.Length - 4);
            }

            return new Uri(value, UriKind.Absolute);
        }
    }
}
