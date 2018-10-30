using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Configuration;

namespace NuKeeper.Engine
{
    public interface IForkFinder
    {
        Task<ForkData> FindPushFork(ForkMode forkMode, string userName, ForkData fallbackFork);
    };
}
