using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;

namespace NuKeeper.Github
{
    public interface IGithub
    {
        Task<string> GetCurrentUser();

        Task OpenPullRequest(string repositoryOwner, string repositoryName, NewPullRequest request);

        Task<IReadOnlyList<Repository>> GetRepositoriesForOrganisation(string organisationName);

        Task<Repository> GetUserRepository(string userName, string repositoryName);
    }
}
