using NuGet.Packaging.Core;
using NuKeeper.Logging;
using System.IO;

namespace NuKeeper.Engine.FilesUpdate
{
    public class ConfigFilesUpdater
    {
        private readonly IConfigFileFinder _finder;
        private readonly INuKeeperLogger _logger;

        public ConfigFilesUpdater(IConfigFileFinder finder)
        {
            _finder = finder;
        }

        public void Update(PackageIdentity from, PackageIdentity to)
        {
            var updater = new ConfigFileUpdater(from, to);

            var files = _finder.FindConfigFiles();

            foreach (var file in files)
            {
                ApplyUpdate(file, updater);
            }
        }

        private void ApplyUpdate(FileInfo file, ConfigFileUpdater updater)
        {
            var fileContents = File.ReadAllText(file.FullName);
            var updatedContent = updater.ApplyUpdate(fileContents);
            if (fileContents != updatedContent)
            {
                File.WriteAllText(file.FullName, updatedContent);
                _logger.Info($"Updated Assembly Binding Redirect in config file {file.FullName}");
            }
        }
    }
}
