using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstract;
using NuKeeper.Abstract.Engine;

namespace NuKeeper.Github.Engine
{
    public static class GitHubExtensions
    {
        public static async Task CreatePullRequest(
            this IClient gitHubClient,
            IRepositoryData repository,
            string title,
            string body,
            string branchWithChanges,
            IEnumerable<string> labels)
        {
            string qualifiedBranch;
            if (repository.Pull.Owner == repository.Push.Owner)
            {
                qualifiedBranch = branchWithChanges;
            }
            else
            {
                qualifiedBranch = repository.Push.Owner + ":" + branchWithChanges;
            }

            var pr = new GithubPullRequest(title, qualifiedBranch, repository.DefaultBranch) { Body = body };

            await gitHubClient.OpenPullRequest(repository.Pull, pr, labels);
        }
    }
}
