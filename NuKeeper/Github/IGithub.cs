using System.Threading.Tasks;

namespace NuKeeper.Github
{
    interface IGithub
    {
        Task<OpenPullRequestResult> OpenPullRequest(OpenPullRequestRequest request);
    }
}
