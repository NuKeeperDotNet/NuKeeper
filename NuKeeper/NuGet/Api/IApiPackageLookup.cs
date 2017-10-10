using System.Threading.Tasks;
using NuGet.Packaging.Core;

namespace NuKeeper.NuGet.Api
{
    public interface IApiPackageLookup
    {
        Task<VersionUpdate> FindVersionUpdate(PackageIdentity package, VersionChange allowedChange);
    }
}
