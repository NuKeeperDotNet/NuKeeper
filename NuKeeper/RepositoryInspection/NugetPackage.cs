using NuGet.Versioning;

namespace NuKeeper.RepositoryInspection
{
    public class NugetPackage
    {
        public NugetPackage(string id, NuGetVersion version, PackagePath packagePath)
        {
            Id = id;
            Version = version;
            Path = packagePath;
        }

        public NugetPackage(string id, string version, PackagePath packagePath) 
            : this(id, new NuGetVersion(version), packagePath)
        {
        }

        public string Id { get; }
        public NuGetVersion Version { get; }
        public PackagePath Path { get; }
    }
}