using NuGet.Versioning;

namespace NuKeeper.RepositoryInspection
{
    public class NuGetPackage
    {
        public NuGetPackage(string id, NuGetVersion version)
        {
            Id = id;
            Version = version;
        }

        public NuGetPackage(string id, string version) : this(id, new NuGetVersion(version))
        {
        }

        public string Id { get; }
        public NuGetVersion Version { get; }
        public string SourceFilePath { get; set; }
    }
}