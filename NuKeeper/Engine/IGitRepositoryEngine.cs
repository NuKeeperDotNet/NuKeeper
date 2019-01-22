using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.Engine
{
    public interface IGitRepositoryEngine
    {
        Task<int> Run(RepositorySettings repository,
            GitUsernamePasswordCredentials credentials,
            SettingsContainer settings, User user);
    }
}
