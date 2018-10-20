using System.Threading.Tasks;
using NuKeeper.Abstract.Configuration;

namespace NuKeeper.Abstract.Engine
{
    public interface IRepositoryFilter
    {
        Task<bool> ContainsDotNetProjects(IRepositorySettings repository);
    }
}
