using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Process
{
    public interface INuGetUpdater
    {
        Task UpdatePackage(PackageIdentity newPackage, PackageInProject currentPackage);
    }
}