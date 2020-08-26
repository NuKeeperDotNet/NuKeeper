using NuKeeper.Abstractions.NuGetApi;

namespace NuKeeper.Inspection.NuGetApi
{
    public interface IPackageLookupResultReporter
    {
        void Report(PackageLookupResult lookupResult);
    }
}
