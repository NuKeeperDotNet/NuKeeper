using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstract.Engine;

namespace NuKeeper.Abstract
{
    public interface IClient
    {
        Task Initialise(IAuthSettings settings);

        Task<IAccount> GetCurrentUser();

        Task CreatePullRequest(IRepositoryData repository, string title, string body, string branchWithChanges,
            IEnumerable<string> labels);

        Task<IPullRequest> OpenPullRequest(IForkData target, INewPullRequest request, IEnumerable<string> labels);

        Task<IReadOnlyList<IOrganization>> GetOrganizations();

        Task<IReadOnlyList<IRepository>> GetRepositoriesForOrganisation(string organisationName);

        Task<IRepository> GetUserRepository(string userName, string repositoryName);

        Task<IRepository> MakeUserFork(string owner, string repositoryName);

        Task<IBranch> GetRepositoryBranch(string userName, string repositoryName, string branchName);

        Task<ISearchCodeResult> Search(ISearchCodeRequest search);
    }
}
