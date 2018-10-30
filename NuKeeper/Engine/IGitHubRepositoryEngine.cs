using LibGit2Sharp;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Engine
{
    public interface IGitHubRepositoryEngine
    {
        Task<int> Run(RepositorySettings repository,
            UsernamePasswordCredentials gitCreds, Identity userIdentity,
            SettingsContainer settings);
    }
}
