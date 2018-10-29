using NuKeeper.Abstractions;

namespace NuKeeper.Github.Mappings
{
    public class GithubOrganization : Organization
    {

        public GithubOrganization(Octokit.Organization organization)
            : base(organization.Name, organization.Login)
        {
        }
    }
}
