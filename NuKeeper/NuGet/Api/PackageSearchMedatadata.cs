using System;
using NuGet.Packaging.Core;

namespace NuKeeper.NuGet.Api
{
    public class PackageSearchMedatadata
    {
        public PackageSearchMedatadata(
            PackageIdentity identity,
            string source)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException(nameof(source));
            }

            Identity = identity;
            Source = source;
        }

        public PackageIdentity Identity { get; }
        public string Source { get; }
    }
}