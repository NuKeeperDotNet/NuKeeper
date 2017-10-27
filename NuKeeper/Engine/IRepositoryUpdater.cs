using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Git;

namespace NuKeeper.Engine
{
    public interface IRepositoryUpdater
    {
        Task Run(IGitDriver git, RepositorySettings settings);
    }
}