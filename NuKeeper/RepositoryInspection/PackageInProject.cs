using NuGet.Versioning;

namespace NuKeeper.RepositoryInspection
{
    public class PackageInProject
    {
        public PackageInProject(string id, NuGetVersion version, PackagePath path, PackageReferenceType packageReferenceType)
        {
            Id = id;
            Version = version;
            Path = path;
            PackageReferenceType = packageReferenceType;
        }

        public PackageInProject(string id, string version, PackagePath path, PackageReferenceType packageReferenceType) :
            this(id, new NuGetVersion(version), path, packageReferenceType)
        {
        }

        public PackageReferenceType PackageReferenceType { get; }
        public string Id { get; }
        public NuGetVersion Version { get; }
        public PackagePath Path { get; }
    }
}