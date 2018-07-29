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

        Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName);

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

        IGitHubAccount Owner { get; }
        IGitHubRepositoryPermissions Permissions { get; }
        IRepository Parent { get; }
    }

    public interface IGitHubRepositoryPermissions
    {
        bool Push { get; }
    }

    internal class OctokitRepository : Repository, IRepository
    {
        public OctokitRepository(string cloneUrl, string name, bool isFork, string htmlUrl,
            IGitHubAccount owner,
            IGitHubRepositoryPermissions permissions, IRepository parent)
        {
            CloneUrl = cloneUrl;
            Name = name;
            Fork = isFork;
            HtmlUrl = htmlUrl;
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
                new OctokitGitHubUser(repository?.Owner),
                new OctokitGitHubRepositoryPermissions(repository?.Permissions),
                repository?.Parent != null ? new OctokitRepository(repository.Parent) : null)
        {
        }

        public new IGitHubAccount Owner { get; }
        public new IGitHubRepositoryPermissions Permissions { get; }
        public new IRepository Parent { get; }
    }

    internal class OctokitGitHubRepositoryPermissions : RepositoryPermissions, IGitHubRepositoryPermissions
    {
        public OctokitGitHubRepositoryPermissions(RepositoryPermissions repositoryPermissions)
        {
            Admin = repositoryPermissions?.Admin ?? false;
            Pull = repositoryPermissions?.Pull ?? false;
            Push = repositoryPermissions?.Push ?? false;
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
