using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Update
{
    public interface IUpdateRunner
    {
        Task Update(PackageUpdateSet updateSet);
    }
}
