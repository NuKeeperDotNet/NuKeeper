using NuKeeper.Abstractions;

namespace NuKeeper.Github.Mappings
{
    public class GithubBranch : Branch
    {
        public GithubBranch(Octokit.Branch branch)
        {
            var a = branch.Name;
            //This is just used to see if the branch exists...
        }
    }
}
