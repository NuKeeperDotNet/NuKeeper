namespace NuKeeper.RepositoryInspection
{
    public class NugetPackage
    {
        public NugetPackage(string id, string version)
        {
            Id = id;
            Version = version;
        }

        public string Id { get; }
        public string Version { get; }
    }
}