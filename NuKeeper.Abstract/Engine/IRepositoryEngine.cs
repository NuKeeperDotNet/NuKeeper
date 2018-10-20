using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Abstract.Configuration;

namespace NuKeeper.Abstract.Engine
{
    public interface IRepositoryEngine
    {
        Task<int> Run(IRepositorySettings repository,
            UsernamePasswordCredentials gitCreds, Identity userIdentity,
            ISettingsContainer settings);
    }
}
