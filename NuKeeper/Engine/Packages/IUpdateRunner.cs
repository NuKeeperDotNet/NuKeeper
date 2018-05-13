using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public interface IUpdateRunner
    {
        Task Update(PackageUpdateSet updateSet);
    }
}
