using System.Collections.Generic;
using System.IO;
using NuGet.Configuration;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;

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
                _logger.Verbose($"Reading file {Path.Combine(file.Root, file.FileName)} for package sources");
            }

            var enabledSources = SettingsUtility.GetEnabledSources(settings);

            return ReadFromFile(enabledSources);
        }

        private NuGetSources ReadFromFile(IEnumerable<PackageSource> enabledSources)
        {
            var sources = new List<string>();

            foreach (var source in enabledSources)
            {
                _logger.Verbose(
                    $"Read [{source.Name}] : {source.SourceUri} from file: {Path.Combine(source.Origin.Root, source.Origin.FileName)}");
                sources.Add(source.SourceUri.ToString());
            }

            return new NuGetSources(sources);
        }
    }
}
