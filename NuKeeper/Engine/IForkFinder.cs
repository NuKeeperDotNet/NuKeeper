using System.Threading.Tasks;

namespace NuKeeper.Engine
{
    public interface IForkFinder
    {
        Task<ForkData> FindPushFork(string userName, ForkData fallbackFork);
    };
}
