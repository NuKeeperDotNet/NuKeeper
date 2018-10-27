using NuKeeper.Abstract;
using Octokit;
using System.Collections.Generic;

namespace NuKeeper.Github.Mappings
{
    public class GithubSearchCodeResult : SearchCodeResult, ISearchCodeResult
    {
        public GithubSearchCodeResult(int totalCount, bool incompleteResults, IReadOnlyList<SearchCode> items)
            : base(totalCount, incompleteResults, items)
        {

        }
        public new int TotalCount => base.TotalCount;
    }
}
