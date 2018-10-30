using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Engine
{
    public interface IForkFinder
    {
        Task<ForkData> FindPushFork(ForkMode forkMode, string userName, ForkData fallbackFork);
    };
}
