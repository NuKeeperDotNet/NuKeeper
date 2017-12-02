using System.Threading.Tasks;
using NuKeeper.Git;

namespace NuKeeper.Engine
{
    public interface IRepositoryUpdater
    {
        Task Run(IGitDriver git, RepositorySpec repository);
    }
}