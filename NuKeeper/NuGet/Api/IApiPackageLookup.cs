using System.Threading.Tasks;

namespace NuKeeper.NuGet.Api
{
    public interface IApiPackageLookup
    {
        Task<PackageSearchMedatadataWithSource> LookupLatest(string packageName);
    }
}
