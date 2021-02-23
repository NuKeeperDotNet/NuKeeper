using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ICollaborationPlatform
    {
        void Initialise(AuthSettings settings);

        Task<User> GetCurrentUser();

        Task<bool> PullRequestExists(ForkData target, string headBranch, string baseBranch);

        Task OpenPullRequest(ForkData target, PullRequestRequest request, IEnumerable<string> labels);

        Task<IReadOnlyList<Organization>> GetOrganizations();

        Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName);

        Task<Repository> GetUserRepository(string userName, string repositoryName);

        Task<Repository> MakeUserFork(string owner, string repositoryName);

        Task<bool> RepositoryBranchExists(string userName, string repositoryName, string branchName);

        Task<SearchCodeResult> Search(SearchCodeRequest search);
        Task<int> GetNumberOfOpenPullRequests(string projectName, string repositoryName);
    }
}
