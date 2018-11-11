using System.Threading.Tasks;
using NuKeeper.Abstractions.DTOs;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface IForkFinder
    {
        Task<ForkData> FindPushFork(string userName, ForkData fallbackFork);
    };
}
