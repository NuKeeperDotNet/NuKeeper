using NuGet.Packaging.Core;

namespace NuKeeper.NuGet.Api
{
    public class PackageSearchMedatadata
    {
        public PackageSearchMedatadata(
            PackageIdentity identity,
            string source)
        {
            Identity = identity;
            Source = source;
        }

        public PackageIdentity Identity { get; }
        public string Source { get; }
    }
}