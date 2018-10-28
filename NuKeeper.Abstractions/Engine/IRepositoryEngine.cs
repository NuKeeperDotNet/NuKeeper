using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.Engine
{
    public interface IRepositoryEngine
    {
        Task<int> Run(IRepositorySettings repository,
            UsernamePasswordCredentials gitCreds, Identity userIdentity,
            ISettingsContainer settings);
    }
}
