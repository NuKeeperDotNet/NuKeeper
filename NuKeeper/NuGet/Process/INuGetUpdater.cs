using System.Threading.Tasks;
using NuKeeper.NuGet.Api;

namespace NuKeeper.NuGet.Process
{
    public interface INuGetUpdater
    {
        Task UpdatePackage(PackageUpdate update);
    }
}