using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuKeeper.Inspection.RepositoryInspection
{
    public class PackageInProject
    {
        public PackageInProject(PackageIdentity identity,
            PackagePath path,
            IEnumerable<string> projectReferences = null)
        {
            Identity = identity;
            Path = path;
            ProjectReferences = projectReferences?.ToList() ?? new List<string>();
        }

        public PackageInProject(string id, string version, PackagePath path) :
            this(new PackageIdentity(id, new NuGetVersion(version)), path, null)
        {
        }

        public PackageIdentity Identity { get; }
        public PackagePath Path { get; }

        public string Id => Identity.Id;
        public NuGetVersion Version => Identity.Version;

        public bool IsPrerelease => Identity.Version.IsPrerelease;

        public IReadOnlyCollection<string> ProjectReferences { get; }
    }
}
