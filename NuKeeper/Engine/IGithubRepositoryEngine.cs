using LibGit2Sharp;
using NuKeeper.Configuration;
using System.Threading.Tasks;

namespace NuKeeper.Engine
{
    public interface IGithubRepositoryEngine
    {
        Task Run(RepositorySettings repository, UsernamePasswordCredentials gitCreds, Identity userIdentity);
    }
}
