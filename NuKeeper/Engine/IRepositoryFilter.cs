using System.Threading.Tasks;
using NuKeeper.Configuration;

namespace NuKeeper.Engine
{
    public interface IRepositoryFilter
    {
        Task<bool> ShouldSkip(RepositorySettings repository);
    }
}
