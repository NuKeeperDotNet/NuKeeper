using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.NuGet;

namespace NuKeeper.Abstractions
{
    public class PackageInProject
    {
        public PackageInProject(PackageVersionRange packageVersionRange,
            PackagePath path)
        {
            Identity = packageVersionRange.SingleVersionIdentity();
            Path = path;
        }

        public PackageIdentity Identity { get; }

        public PackagePath Path { get; }

        public NuGetVersion Version => Identity.Version;
    }
}
