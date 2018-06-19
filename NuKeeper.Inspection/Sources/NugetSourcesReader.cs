using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Inspection.Sources
{
    public class NugetSourcesReader : INugetSourcesReader
    {
        private readonly NuGetSources _fromSettings;
        private readonly NugetConfigFileReader _reader;
        private readonly INuKeeperLogger _logger;

        public NugetSourcesReader(
            NuGetSources fromSettings,
            NugetConfigFileReader reader,
            INuKeeperLogger logger)
        {
            _fromSettings = fromSettings;
            _reader = reader;
            _logger = logger;
        }

        public NuGetSources Read(IFolder workingFolder)
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

            _logger.Verbose("Using default global NuGet feed");
            return NuGetSources.GlobalFeed;
        }
    }
}
