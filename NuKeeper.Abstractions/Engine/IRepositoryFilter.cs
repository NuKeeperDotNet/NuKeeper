using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.Engine
{
    public interface IRepositoryFilter
    {
        Task<bool> ContainsDotNetProjects(IRepositorySettings repository);
    }
}
