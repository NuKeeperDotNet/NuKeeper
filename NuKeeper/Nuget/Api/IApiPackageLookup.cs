using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;

namespace NuKeeper.Nuget.Api
{
    public interface IApiPackageLookup
    {
        Task<IPackageSearchMetadata> LookupLatest(string packageName);
    }
}
