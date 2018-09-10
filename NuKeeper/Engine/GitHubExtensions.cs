using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.GitHub;
using Octokit;

namespace NuKeeper.Engine
{
    public static class GitHubExtensions
    {
        public static async Task CreatePullRequest(
            this IGitHub gitHub,
            RepositoryData repository,
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

            var pr = new NewPullRequest(title, qualifiedBranch, repository.DefaultBranch) { Body = body };

            await gitHub.OpenPullRequest(repository.Pull, pr, labels);
        }
    }
}
