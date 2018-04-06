using NuGet.Packaging.Core;
using NuKeeper.Files;
using NuKeeper.Logging;
using System.IO;

namespace NuKeeper.Engine.FilesUpdate
{
    public class ConfigFilesUpdater: IConfigFilesUpdater
    {
        private readonly IConfigFileFinder _finder;
        private readonly INuKeeperLogger _logger;

        public ConfigFilesUpdater(IConfigFileFinder finder, INuKeeperLogger logger)
        {
            _finder = finder;
            _logger = logger;
        }

        public void Update(IFolder folder, PackageIdentity from, PackageIdentity to)
        {
            var files = _finder.FindInFolder(folder);

            _logger.Verbose($"Found {files.Count} config files");

            if (files.Count == 0)
            {
                return;
            }

            var updater = new ConfigFileUpdater(from, to);

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
