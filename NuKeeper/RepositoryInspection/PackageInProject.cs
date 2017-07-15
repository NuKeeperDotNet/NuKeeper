using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuKeeper.RepositoryInspection
{
    public class PackageInProject
    {
        public PackageInProject(PackageIdentity identity, PackagePath path)
        {
            Identity = identity;
            Path = path;
        }

        public PackageInProject(string id, string version, PackagePath path) :
            this(new PackageIdentity(id, new NuGetVersion(version)), path)
        {
        }

        public PackageIdentity Identity { get; }
        public PackagePath Path { get; }

        public string Id => Identity.Id;
        public NuGetVersion Version => Identity.Version;

        public PackageReferenceType PackageReferenceType => Path.PackageReferenceType;
    }
}