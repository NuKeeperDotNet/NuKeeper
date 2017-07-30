using System.Collections.Generic;
using System.Threading.Tasks;
using Octokit;
using NuKeeper.Github.Models;

namespace NuKeeper.Github
{
    public interface IGithub
    {
        Task<string> GetCurrentUser();
        Task OpenPullRequest(OpenPullRequestRequest request);
        Task<IReadOnlyList<GithubRepository>> GetRepositoriesForOrganisation(string organisationName);
    }
}
