using System.Threading.Tasks;
using NuKeeper.Git;

namespace NuKeeper.Engine
{
    public interface IRepositoryUpdater
    {
        Task<int> Run(IGitDriver git, RepositoryData repository);
    }
}
