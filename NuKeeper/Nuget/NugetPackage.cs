namespace NuKeeper.Nuget
{
    public class NugetPackage
    {
        public NugetPackage(string id, string version, string targetFramework)
        {
            Id = id;
            Version = version;
            TargetFramework = targetFramework;
        }

        public string Id { get; }
        public string Version { get; }
        public string TargetFramework { get; }
    }
}