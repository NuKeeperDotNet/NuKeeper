using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Engine
{
    public interface IRepositoryFilter
    {
        Task<bool> ContainsDotNetProjects(RepositorySettings repository);
    }
}
