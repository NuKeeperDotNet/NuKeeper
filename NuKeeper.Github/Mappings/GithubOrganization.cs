using NuKeeper.Abstract;
using Octokit;

namespace NuKeeper.Github.Mappings
{
    public class GithubOrganization: IOrganization
    {
        private readonly Organization _organization;

        public GithubOrganization(Organization organization)
        {
            _organization = organization;
        }

        public string Name => _organization.Name;
        public string Login => _organization.Login;
    }
}
