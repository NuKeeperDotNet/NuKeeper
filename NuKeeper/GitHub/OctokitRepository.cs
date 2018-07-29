using Octokit;

namespace NuKeeper.GitHub
{
    internal class OctokitRepository : Repository, IRepository
    {
        public OctokitRepository(string cloneUrl, string name, bool isFork, string htmlUrl,
            bool archived,
            IGitHubAccount owner,
            IGitHubRepositoryPermissions permissions, IRepository parent)
        {
            CloneUrl = cloneUrl;
            Name = name;
            Fork = isFork;
            HtmlUrl = htmlUrl;
            Archived = archived;
            Owner = owner;
            Permissions = permissions;
            Parent = parent;
        }

        internal OctokitRepository(Repository repository)
            : this(
                repository?.CloneUrl,
                repository?.Name,
                repository?.Fork ?? false,
                repository?.HtmlUrl,
                repository?.Archived ?? false,
                new OctokitUser(repository?.Owner),
                new OctokitRepositoryPermissions(repository?.Permissions),
                repository?.Parent != null ? new OctokitRepository(repository.Parent) : null)
        {
        }

        public new IGitHubAccount Owner { get; }
        public new IGitHubRepositoryPermissions Permissions { get; }
        public new IRepository Parent { get; }
    }
}
