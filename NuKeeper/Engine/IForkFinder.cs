using System.Threading.Tasks;

namespace NuKeeper.Engine
{
    public interface IForkFinder
    {
        Task<ForkData> PushFork(string userName, string repositoryName, ForkData fallbackFork);
    };
}
