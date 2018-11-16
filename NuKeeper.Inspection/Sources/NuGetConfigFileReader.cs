using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet.Configuration;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.Files;

namespace NuKeeper.Inspection.Sources
{
    public class NuGetConfigFileReader
    {
        private readonly INuKeeperLogger _logger;

        public NuGetConfigFileReader(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public NuGetSources ReadNugetSources(IFolder workingFolder)
        {
            var settings = Settings.LoadDefaultSettings(workingFolder.FullPath);

            foreach (var file in settings.Priority)
            {
                _logger.Detailed($"Reading file {Path.Combine(file.Root, file.FileName)} for package sources");
            }

            var enabledSources = SettingsUtility.GetEnabledSources(settings).ToList();

            return ReadFromFile(enabledSources);
        }

        private NuGetSources ReadFromFile(IReadOnlyCollection<PackageSource> sources)
        {
            foreach (var source in sources)
            {
                _logger.Detailed(
                    $"Read [{source.Name}] : {source.SourceUri} from file: {Path.Combine(source.Origin.Root, source.Origin.FileName)}");
            }

            return new NuGetSources(sources);
        }
    }
}
