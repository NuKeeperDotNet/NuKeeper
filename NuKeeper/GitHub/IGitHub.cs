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
}
