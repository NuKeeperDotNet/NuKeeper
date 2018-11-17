using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.Engine
{
    public interface IRepositoryUpdater
    {
        Task<int> Run(IGitDriver git, RepositoryData repository, SettingsContainer settings);
    }
}
