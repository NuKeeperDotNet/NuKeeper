using System.IO;
using System.Linq;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Inspection.Sources
{
    public class NuGetConfigFileReader
    {
        private readonly NuGetConfigFileParser _parser;
        private readonly INuKeeperLogger _logger;

        public NuGetConfigFileReader(
            NuGetConfigFileParser parser,
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
                _logger.Verbose($"Reading nuget.config file {configFile.FullName} for package sources");
                var fromFile = _parser.Parse(fileContents);
                if (fromFile != null)
                {
                    var itemsText = string.Join(',', fromFile.Items);
                    _logger.Verbose($"Read {fromFile.Items.Count} package sources from file: {itemsText}");
                    return fromFile;
                }
            }

            return null;
        }
    }
}
