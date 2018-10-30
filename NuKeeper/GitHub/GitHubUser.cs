using NuKeeper.Abstractions.DTOs;
using Account = Octokit.Account;

namespace NuKeeper.GitHub
{
    public class GitHubUser : User
    {
        public GitHubUser(Account user)
            : base(user.Login, user.Name, user.Email)
        {
        }
    }
}
