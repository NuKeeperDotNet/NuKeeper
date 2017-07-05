using System.Threading.Tasks;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Nuget
{
    interface INuget
    {
        Task<VersionComparisonResult> CompareVersions(NugetPackage package);
    }
}
