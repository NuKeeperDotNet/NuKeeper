using NuKeeper.Abstractions.DTOs;

namespace NuKeeper.GitHub
{
    public class GitHubSearchCodeResult : SearchCodeResult
    {
        public GitHubSearchCodeResult(Octokit.SearchCodeResult result)
        : base(result.TotalCount)
        {
        }
    }
}
