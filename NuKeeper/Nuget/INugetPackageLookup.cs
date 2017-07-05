using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;

namespace NuKeeper.Nuget
{
    public interface INugetPackageLookup
    {
        Task<IPackageSearchMetadata> LookupLatest(string packageName);
    }
}
