using NuKeeper.Abstract;
using Octokit;

namespace NuKeeper.Github.Mappings
{
    public class GithubOrganization : Organization, IOrganization
    {
        public new string Name => base.Name;
        public new string Login => base.Login;
    }
}
