using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Engine;

namespace NuKeeper.Abstractions
{
    public interface IClient
    {
        Task Initialise(AuthSettings settings);

        Task<User> GetCurrentUser();

        Task CreatePullRequest(IRepositoryData repository, string title, string body, string branchWithChanges,
            IEnumerable<string> labels);

        Task OpenPullRequest(ForkData target, NewPullRequest request, IEnumerable<string> labels);

        Task<IReadOnlyList<Organization>> GetOrganizations();

        Task<IReadOnlyList<IRepository>> GetRepositoriesForOrganisation(string organisationName);

        Task<IRepository> GetUserRepository(string userName, string repositoryName);

        Task<IRepository> MakeUserFork(string owner, string repositoryName);

        Task<Branch> GetRepositoryBranch(string userName, string repositoryName, string branchName);

        Task<SearchCodeResult> Search(SearchCodeRequest search);
    }
}
