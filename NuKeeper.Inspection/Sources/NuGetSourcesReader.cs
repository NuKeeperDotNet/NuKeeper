using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;

namespace NuKeeper.Inspection.Sources
{
    public class NuGetSourcesReader : INuGetSourcesReader
    {
        private readonly NuGetConfigFileReader _reader;
        private readonly INuKeeperLogger _logger;

        public NuGetSourcesReader(
            NuGetConfigFileReader reader,
            INuKeeperLogger logger)
        {
            _reader = reader;
            _logger = logger;
        }

        public NuGetSources Read(IFolder workingFolder, NuGetSources overrideValues)
        {
            if (overrideValues != null)
            {
                return overrideValues;
            }

            var fromConfigFile = _reader.ReadNugetSources(workingFolder);

            if (fromConfigFile != null)
            {
                return fromConfigFile;
            }

            _logger.Detailed("Using default global NuGet feed");
            return NuGetSources.GlobalFeed;
        }
    }
}
