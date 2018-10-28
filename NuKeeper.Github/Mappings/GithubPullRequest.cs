using NuKeeper.Abstractions;
using Octokit;

namespace NuKeeper.Github.Engine
{
    public class GithubPullRequest : INewPullRequest
    {
        private readonly NewPullRequest _newPullRequest;
        public GithubPullRequest(NewPullRequest newPullRequest)
        {
            _newPullRequest = newPullRequest;
        }

        public string Title => _newPullRequest.Title;

        public string Head => _newPullRequest.Head;

        public string BaseRef => _newPullRequest.Base;
    }
}
