using NuGet.Versioning;

namespace NuKeeper.RepositoryInspection
{
    public class NuGetPackage
    {
        public NuGetPackage(string id, NuGetVersion version, PackagePath path, PackageType packageType)
        {
            Id = id;
            Version = version;
            Path = path;
            PackageType = packageType;
        }

        public NuGetPackage(string id, string version, PackagePath path, PackageType packageType) :
            this(id, new NuGetVersion(version), path, packageType)
        {
        }

        public PackageType PackageType { get; }
        public string Id { get; }
        public NuGetVersion Version { get; }
        public PackagePath Path { get; }
    }
}