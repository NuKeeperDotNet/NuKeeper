using System.Threading.Tasks;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Update
{
    public interface IUpdateRunner
    {
        Task Update(PackageUpdateSet updateSet, NuGetSources sources);

        Task Downgrade(PackageUpdateSet updateSet, NuGetSources sources);
    }
}
