using NuGet.Versioning;

namespace NuKeeper.RepositoryInspection
{
    public class NuGetPackage
    {
        public NuGetPackage(string id, NuGetVersion version, PackagePath path)
        {
            Id = id;
            Version = version;
			Path = path;
        }

        public NuGetPackage(string id, string version,PackagePath path): 
		  this(id, new NuGetVersion(version), path)
        {
        }

        public string Id { get; }
        public NuGetVersion Version { get; }
        public PackagePath Path { get; }
    }
}