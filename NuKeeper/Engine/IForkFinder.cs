using System.Threading.Tasks;

namespace NuKeeper.Engine
{
    public interface IForkFinder
    {
        Task<ForkSpec> PushFork(string userName, string repositoryName, ForkSpec fallbackFork);
    };
}
