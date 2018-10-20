using NuKeeper.Abstract;
using Octokit;

namespace NuKeeper.Github.Mappings
{
    public class GithubSearchCodeResult : SearchCodeResult, ISearchCodeResult
    {
        public new int TotalCount => base.TotalCount;
    }
}
