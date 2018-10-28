using NuKeeper.Abstract;
using Octokit;

namespace NuKeeper.Github.Mappings
{
    public class GithubPullRequestInfo : IPullRequest
    {
        private readonly PullRequest _pullRequest;

        public GithubPullRequestInfo(PullRequest pullRequest)
        {
            _pullRequest = pullRequest;
        }
    }
}
