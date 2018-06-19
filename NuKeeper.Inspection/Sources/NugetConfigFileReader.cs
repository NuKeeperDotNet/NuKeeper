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

            using (var fileContents = File.OpenRead(configFile.FullName))
            {
                var fromFile = _parser.Parse(fileContents);
                if (fromFile != null && fromFile.Items.Count > 0)
                {
                    _logger.Verbose($"Read {fromFile.Items.Count} NuGet sources from file {configFile.FullName}");
                    return fromFile;
                }
            }

            return null;
        }
    }
}
