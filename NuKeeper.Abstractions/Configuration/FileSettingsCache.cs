using System.IO;

namespace NuKeeper.Abstractions.Configuration
{
    public class FileSettingsCache : IFileSettingsCache
    {
        private readonly FileSettingsReader _reader;
        private FileSettings _settings;

        public FileSettingsCache(FileSettingsReader reader)
        {
            _reader = reader;
        }

        public FileSettings GetSettings()
        {
            if (_settings == null)
            {
                _settings = _reader.Read(GetFolder());
            }

            return _settings;
        }

        private static string GetFolder()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}
