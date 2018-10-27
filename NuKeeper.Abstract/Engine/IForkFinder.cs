using System.Threading.Tasks;
using NuKeeper.Abstract.Configuration;

namespace NuKeeper.Abstract.Engine
{
    public interface IForkFinder
    {
        Task<IForkData> FindPushFork(ForkMode forkMode, string userName, ForkData fallbackFork);
    };
}
