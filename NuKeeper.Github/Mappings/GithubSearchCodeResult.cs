using NuKeeper.Abstractions;

namespace NuKeeper.Github.Mappings
{
    public class GithubSearchCodeResult : SearchCodeResult
    {

        public GithubSearchCodeResult(Octokit.SearchCodeResult searchCodeResult)
        : base(searchCodeResult.TotalCount)
        {
        }
    }
}
