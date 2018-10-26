using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.AzureDevOps.Models;

namespace NuKeeper.AzureDevOps
{
    public interface IAzureDevOpsClient
    {
        void Initialise(AzureDevopsAuthSettings settings);

        //[Get("_apis/profile/profiles/{id}?api-version=5.0-preview.3")]
        Task<AzureProfile> GetCurrentUser(string id);

        //[Post("_apis/git/repositories/{repositoryId}/pullrequests?api-version=5.0-preview.1")]
        Task<GitPullRequest> OpenPullRequest(string repositoryId, PullRequest request);

        Task<IReadOnlyList<bool>> GetOrganizations();

        Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName);

        Task<Repository> GetUserRepository(string userName, string repositoryName);

        Task<Repository> MakeUserFork(string owner, string repositoryName);

//        Task<Branch> GetRepositoryBranch(string userName, string repositoryName, string branchName);
//
//        Task<SearchCodeResult> Search(SearchCodeRequest search);
    }
}
