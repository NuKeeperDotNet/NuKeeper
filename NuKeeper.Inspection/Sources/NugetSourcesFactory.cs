using NuKeeper.Inspection.Files;

namespace NuKeeper.Inspection.Sources
{
    public class NugetSourcesFactory : INugetSourcesFactory
    {
        private readonly NuGetSources _fromSettings;
        private readonly NugetConfigFileReader _reader;

        private const string DefaultFeed = "https://api.nuget.org/v3/index.json";

        public NugetSourcesFactory(
            NuGetSources fromSettings,
            NugetConfigFileReader reader)
        {
            _fromSettings = fromSettings;
            _reader = reader;
        }

        public NuGetSources ReadNugetSources(IFolder workingFolder)
        {
            if (_fromSettings != null)
            {
                return _fromSettings;
            }

            var fromConfigFile = _reader.ReadNugetSources(workingFolder);

            if (fromConfigFile != null)
            {
                return fromConfigFile;
            }

            return new NuGetSources(DefaultFeed);
        }
    }
}
