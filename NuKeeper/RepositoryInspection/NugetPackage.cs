using NuGet.Versioning;

namespace NuKeeper.RepositoryInspection
{
    public class NugetPackage
    {
        public NugetPackage(string id, NuGetVersion version)
        {
            Id = id;
            Version = version;
        }

        public NugetPackage(string id, string version) : this(id, new NuGetVersion(version))
        {
        }

        public string Id { get; }
        public NuGetVersion Version { get; }
    }
}