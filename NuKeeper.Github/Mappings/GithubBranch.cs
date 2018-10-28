using NuKeeper.Abstract;
using Octokit;

namespace NuKeeper.Github.Mappings
{
    public class GithubBranch : IBranch
    {
        private readonly Branch _branch;

        public GithubBranch(Branch branch)
        {
            _branch = branch;
        }
    }
}
