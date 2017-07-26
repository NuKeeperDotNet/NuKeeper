using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;

namespace NuKeeper.NuGet.Api
{
    public interface IBulkPackageLookup
    {
        Task<Dictionary<string, IPackageSearchMetadata>> LatestVersions(IEnumerable<string> packageIds);
    }
}