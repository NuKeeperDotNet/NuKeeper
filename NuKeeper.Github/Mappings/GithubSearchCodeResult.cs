using NuKeeper.Abstractions;
using Octokit;

namespace NuKeeper.Github.Mappings
{
    public class GithubSearchCodeResult: ISearchCodeResult
    {
        private readonly SearchCodeResult _searchCodeResult;

        public GithubSearchCodeResult(SearchCodeResult searchCodeResult)
        {
            _searchCodeResult = searchCodeResult;
        }
        public int TotalCount => _searchCodeResult.TotalCount;
    }
}
