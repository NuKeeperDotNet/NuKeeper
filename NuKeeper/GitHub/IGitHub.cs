using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Engine;
using Octokit;

namespace NuKeeper.GitHub
{
    public interface IGitHub
    {
        Task<IGitHubAccount> GetCurrentUser();

        Task<PullRequest> OpenPullRequest(ForkData target, NewPullRequest request, IEnumerable<string> labels);

        Task<IReadOnlyList<Organization>> GetOrganizations();

        Task<IReadOnlyList<IRepository>> GetRepositoriesForOrganisation(string organisationName);

        Task<IRepository> GetUserRepository(string userName, string repositoryName);

        Task<IRepository> MakeUserFork(string owner, string repositoryName);

        Task<Branch> GetRepositoryBranch(string userName, string repositoryName, string branchName);

        Task<SearchCodeResult> Search(SearchCodeRequest search);
    }

    public interface IRepository
    {
        string CloneUrl { get; }
        string Name { get; }
        bool Fork { get; }
        string HtmlUrl { get; }
        bool Archived { get; }

        IGitHubAccount Owner { get; }
        IGitHubRepositoryPermissions Permissions { get; }
        IRepository Parent { get; }
    }

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
                new OctokitGitHubUser(repository?.Owner),
                new OctokitGitHubRepositoryPermissions(repository?.Permissions),
                repository?.Parent != null ? new OctokitRepository(repository.Parent) : null)
        {
        }

        public new IGitHubAccount Owner { get; }
        public new IGitHubRepositoryPermissions Permissions { get; }
        public new IRepository Parent { get; }
    }

    public interface IGitHubRepositoryPermissions
    {
        bool Push { get; }
        bool Pull { get; }
    }

    internal class OctokitGitHubRepositoryPermissions : RepositoryPermissions, IGitHubRepositoryPermissions
    {
        internal OctokitGitHubRepositoryPermissions(bool admin, bool push, bool pull)
        {
            Admin = admin;
            Pull = pull;
            Push = push;
        }

        internal OctokitGitHubRepositoryPermissions(RepositoryPermissions repositoryPermissions)
        : this(repositoryPermissions?.Admin ?? false,
            repositoryPermissions?.Pull ?? false,
            repositoryPermissions?.Push ?? false)
        {
        }
    }

    public interface IGitHubAccount
    {
        string Login { get; }
        string Name { get; }
        string Email { get; }
    }

    internal class OctokitGitHubUser : User, IGitHubAccount
    {
        internal OctokitGitHubUser(string login, string name, string email)
        {
            Login = login;
            Name = name;
            Email = email;
        }

        internal OctokitGitHubUser(User user)
            : this(user?.Login, user?.Name, user?.Email)
        {
        }
    }
}
