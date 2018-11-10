using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface IForkFinder
    {
        Task<ForkData> FindPushFork(ForkMode forkMode, string userName, ForkData fallbackFork);
    };
}
