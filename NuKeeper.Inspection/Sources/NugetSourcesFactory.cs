using NuKeeper.Inspection.Files;

namespace NuKeeper.Inspection.Sources
{
    public class NugetSourcesFactory : INugetSourcesFactory
    {
        private readonly NuGetSources _fromSettings;
        private readonly NugetConfigFileReader _reader;

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

            return NuGetSources.GlobalFeed;
        }
    }
}
