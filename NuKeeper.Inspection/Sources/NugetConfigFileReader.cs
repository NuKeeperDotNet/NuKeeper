using System.IO;
using System.Linq;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Inspection.Sources
{
    public class NugetConfigFileReader
    {
        private readonly NugetConfigFileParser _parser;
        private readonly INuKeeperLogger _logger;

        public NugetConfigFileReader(
            NugetConfigFileParser parser,
            INuKeeperLogger logger)
        {
            _parser = parser;
            _logger = logger;
        }

        public NuGetSources ReadNugetSources(IFolder workingFolder)
        {
            var configFile = workingFolder.Find("nuget.config")
                .FirstOrDefault();

            if (configFile == null)
            {
                return null;
            }

            return ReadFromFile(configFile);
        }

        private NuGetSources ReadFromFile(FileInfo configFile)
        {
            using (var fileContents = File.OpenRead(configFile.FullName))
            {
                _logger.Verbose($"Reading nuget.config file {configFile.FullName}");
                var fromFile = _parser.Parse(fileContents);
                if (fromFile != null)
                {
                    _logger.Verbose($"Read {fromFile.Items.Count} NuGet sources from file");
                    return fromFile;
                }
            }

            return null;
        }
    }
}
