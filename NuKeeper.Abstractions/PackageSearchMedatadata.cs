using System;
using NuGet.Packaging.Core;

namespace NuKeeper.Abstractions
{
    public class PackageSearchMedatadata
    {
        public PackageSearchMedatadata(
            PackageIdentity identity,
            Uri sourceUri,
            DateTimeOffset? published)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            SourceUri = sourceUri ?? throw new ArgumentNullException(nameof(sourceUri));
            Published = published;
        }

        public PackageIdentity Identity { get; }

        public Uri SourceUri { get; }

        public DateTimeOffset? Published { get; }
    }
}
