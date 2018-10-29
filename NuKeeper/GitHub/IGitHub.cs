using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Engine;
using Octokit;

namespace NuKeeper.GitHub
{
    public interface IGitHub
    {
        void Initialise(AuthSettings settings);

        Task<Account> GetCurrentUser();

        Task<PullRequest> OpenPullRequest(ForkData target, NewPullRequest request, IEnumerable<string> labels);

        Task<IReadOnlyList<Organization>> GetOrganizations();

        Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName);

        Task<Repository> GetUserRepository(string userName, string repositoryName);

        Task<Repository> MakeUserFork(string owner, string repositoryName);

        Task<Branch> GetRepositoryBranch(string userName, string repositoryName, string branchName);

        Task<SearchCodeResult> Search(SearchCodeRequest search);
    }
}
