using NuKeeper.Abstractions;

namespace NuKeeper.Github.Mappings
{
    public class GithubPullRequest : NewPullRequest
    {
        public GithubPullRequest(Octokit.NewPullRequest newPullRequest)
            : base(newPullRequest.Title, newPullRequest.Head, newPullRequest.Base)
        {
        }
    }
}
