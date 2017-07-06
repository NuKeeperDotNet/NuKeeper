using System.Threading.Tasks;
using NuKeeper.Nuget.Api;

namespace NuKeeper.Nuget.Process
{
    public interface INugetUpdater
    {
        Task UpdatePackage(PackageUpdate update);
    }
}