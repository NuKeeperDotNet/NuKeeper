using Octokit;

namespace NuKeeper.GitHub
{
    internal class OctokitUser : User, IGitHubAccount
    {
        internal OctokitUser(string login, string name, string email)
        {
            Login = login;
            Name = name;
            Email = email;
        }

        internal OctokitUser(User user)
            : this(user?.Login, user?.Name, user?.Email)
        {
        }
    }
}
