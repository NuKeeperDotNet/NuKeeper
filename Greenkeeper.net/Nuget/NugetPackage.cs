namespace NuKeeper.Nuget
{
    public class NugetPackage
    {
        public NugetPackage(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public string Name { get; }
        public string Version { get; }
    }
}