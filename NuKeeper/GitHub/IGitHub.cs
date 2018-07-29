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

        Task<Repository> GetUserRepository(string userName, string repositoryName);

        Task<Repository> MakeUserFork(string owner, string repositoryName);

        Task<Branch> GetRepositoryBranch(string userName, string repositoryName, string branchName);

        Task<SearchCodeResult> Search(SearchCodeRequest search);
    }

    public interface IGitHubAccount
    {
        string Login { get; }
        string Name { get; }
        string Email { get; }
    }

    public class OctokitGitHubUser : User, IGitHubAccount
    {
        public OctokitGitHubUser(string login, string name, string email)
        {
            Login = login;
            Name = name;
            Email = email;
        }
    }
}
