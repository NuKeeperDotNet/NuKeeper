using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface IForkFinder
    {
        Task<ForkData> FindPushFork(string userName, ForkData fallbackFork);
    };
}
