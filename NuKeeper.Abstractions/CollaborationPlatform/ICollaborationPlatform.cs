using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ICollaborationPlatform
    {
        void Initialise(AuthSettings settings);

        Task<User> GetCurrentUser();

        Task OpenPullRequest(ForkData target, PullRequestRequest request, IEnumerable<string> labels);

        Task<IReadOnlyList<Organization>> GetOrganizations();

        Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName);

        Task<Repository> GetUserRepository(string userName, string repositoryName);

        Task<Repository> MakeUserFork(string owner, string repositoryName);

        Task<bool> RepositoryBranchExists(string userName, string repositoryName, string branchName);

        Task<SearchCodeResult> Search(SearchCodeRequest search);
    }
}
