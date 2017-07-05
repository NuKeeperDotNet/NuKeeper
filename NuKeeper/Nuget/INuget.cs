using System.Threading.Tasks;

namespace NuKeeper.Nuget
{
    interface INuget
    {
        Task<VersionComparisonResult> CompareVersions(NugetPackage package);

        Task UpdatePackage(NugetPackage package, string pathToSolutionFile);
    }
}
