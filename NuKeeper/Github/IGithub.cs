using System.Threading.Tasks;

namespace NuKeeper.Github
{
    public interface IGithub
    {
        Task<OpenPullRequestResult> OpenPullRequest(OpenPullRequestRequest request);
    }
}
