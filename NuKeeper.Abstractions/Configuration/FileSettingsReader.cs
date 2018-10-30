using System;
using System.IO;
using Newtonsoft.Json;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Abstractions.Configuration
{
    public class FileSettingsReader
    {
        private readonly INuKeeperLogger _logger;

        public FileSettingsReader(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public FileSettings Read(string folder)
        {
            const string fileName = "nukeeper.settings.json";

            var fullPath = Path.Combine(folder, fileName);

            if (File.Exists(fullPath))
            {
                return ReadFile(fullPath);
            }

            return FileSettings.Empty();
        }

        private FileSettings ReadFile(string fullPath)
        {
            try
            {
                var contents = File.ReadAllText(fullPath);
                var result = JsonConvert.DeserializeObject<FileSettings>(contents);
                _logger.Detailed($"Read settings file at {fullPath}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot read settings file at {fullPath}", ex);
                return FileSettings.Empty();
            }
        }
    }
}
