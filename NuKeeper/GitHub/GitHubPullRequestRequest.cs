using NuKeeper.Abstractions.DTOs;
using Account = Octokit.Account;

namespace NuKeeper.GitHub
{
    public class GitHubPullRequestRequest : PullRequestRequest
    {
        public GitHubPullRequestRequest(Octokit.NewPullRequest pullRequest)
            :base(pullRequest.Head, pullRequest.Title,pullRequest.Base)
        {
        }
    }
}
