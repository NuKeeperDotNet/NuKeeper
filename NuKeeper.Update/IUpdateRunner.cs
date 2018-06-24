using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Update
{
    public interface IUpdateRunner
    {
        Task Update(PackageUpdateSet updateSet, NuGetSources sources);
    }
}
