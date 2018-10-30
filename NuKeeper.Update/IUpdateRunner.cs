using System.Threading.Tasks;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Update
{
    public interface IUpdateRunner
    {
        Task Update(PackageUpdateSet updateSet, NuGetSources sources);
    }
}
