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
            PackagePath path,
            IEnumerable<string> projectReferences = null)
        {
            PackageVersionRange = packageVersionRange;
            Path = path;
            ProjectReferences = projectReferences?.ToList() ?? new List<string>();
        }

        public PackageInProject(string id, string versionRange, PackagePath path) :
            this(new PackageVersionRange(id, VersionRange.Parse(versionRange)), path, null)
        {
        }

        public PackageIdentity Identity => PackageVersionRange.SingleVersionIdentity();

        public PackageVersionRange PackageVersionRange { get; }

        public PackagePath Path { get; }

        public string Id => PackageVersionRange.Id;
        public VersionRange Range => PackageVersionRange.Version;

        public NuGetVersion Version => Identity.Version;

        public bool IsPrerelease => Identity.Version.IsPrerelease;

        public IReadOnlyCollection<string> ProjectReferences { get; }
    }
}
