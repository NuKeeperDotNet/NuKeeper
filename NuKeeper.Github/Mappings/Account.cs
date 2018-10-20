using NuKeeper.Abstract;
using Octokit;

namespace NuKeeper.Github.Mappings
{
    public class GithubAccount : User, IAccount
    {
        public new string Login => base.Login;
        public new string Name => base.Name;
        public new string Email => base.Email;
    }
}
