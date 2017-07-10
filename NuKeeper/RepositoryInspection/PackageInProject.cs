using NuGet.Versioning;

namespace NuKeeper.RepositoryInspection
{
    public class PackageInProject
    {
        public PackageInProject(string id, NuGetVersion version, PackagePath path)
        {
            Id = id;
            Version = version;
            Path = path;
        }

        public PackageInProject(string id, string version, PackagePath path) :
            this(id, new NuGetVersion(version), path)
        {
        }

        public string Id { get; }
        public NuGetVersion Version { get; }
        public PackagePath Path { get; }

        public PackageReferenceType PackageReferenceType => Path.PackageReferenceType;
    }
}