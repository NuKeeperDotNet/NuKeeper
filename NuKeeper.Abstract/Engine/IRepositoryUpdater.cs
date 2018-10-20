using System.Threading.Tasks;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Git;

namespace NuKeeper.Abstract.Engine
{
    public interface IRepositoryUpdater
    {
        Task<int> Run(IGitDriver git, RepositoryData repository, ISettingsContainer settings);
    }
}
