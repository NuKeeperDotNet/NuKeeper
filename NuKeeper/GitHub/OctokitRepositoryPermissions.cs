using Octokit;

namespace NuKeeper.GitHub
{
    internal class OctokitRepositoryPermissions : RepositoryPermissions, IGitHubRepositoryPermissions
    {
        internal OctokitRepositoryPermissions(bool admin, bool push, bool pull)
        {
            Admin = admin;
            Pull = pull;
            Push = push;
        }

        internal OctokitRepositoryPermissions(RepositoryPermissions repositoryPermissions)
            : this(repositoryPermissions?.Admin ?? false,
                repositoryPermissions?.Pull ?? false,
                repositoryPermissions?.Push ?? false)
        {
        }
    }
}
