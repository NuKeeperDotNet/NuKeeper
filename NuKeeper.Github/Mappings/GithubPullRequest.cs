using NuKeeper.Abstract;
using Octokit;

namespace NuKeeper.Github.Engine
{
    public class GithubPullRequest : NewPullRequest, INewPullRequest
    {
        public GithubPullRequest(string title, string head, string baseRef) : base(title, head, baseRef)
        {
        }
    }
}
