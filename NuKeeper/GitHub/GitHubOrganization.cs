using NuKeeper.Abstractions.DTOs;
using Account = Octokit.Account;

namespace NuKeeper.GitHub
{
    public class GitHubOrganization : Organization
    {
        public GitHubOrganization(Account account)
            : base(account.Name, account.Login)
        {
        }
    }
}
