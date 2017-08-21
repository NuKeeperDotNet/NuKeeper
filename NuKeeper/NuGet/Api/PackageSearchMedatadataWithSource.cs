using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace NuKeeper.NuGet.Api
{
    public class PackageSearchMedatadataWithSource
    {
        private readonly IPackageSearchMetadata _originalMetadata;

        public PackageSearchMedatadataWithSource(string source, IPackageSearchMetadata originalMetadata)
        {
            _originalMetadata = originalMetadata;
            Source = source;
        }

        public string Source { get; }

        public PackageIdentity Identity => _originalMetadata.Identity;
    }
}