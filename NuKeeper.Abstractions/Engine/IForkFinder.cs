using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.Engine
{
    public interface IForkFinder
    {
        Task<IForkData> FindPushFork(ForkMode forkMode, string userName, ForkData fallbackFork);
    };
}
