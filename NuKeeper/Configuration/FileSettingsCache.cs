using System.IO;
using NuKeeper.Inspection.Files;

namespace NuKeeper.Configuration
{
    public class FileSettingsCache : IFileSettingsCache
    {
        private readonly FileSettingsReader _reader;
        private FileSettings _settings;

        public FileSettingsCache(FileSettingsReader reader)
        {
            _reader = reader;
        }

        public FileSettings Get()
        {
            if (_settings == null)
            {
                _settings = _reader.Read(GetFolder());
            }

            return _settings;
        }

        private string GetFolder()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}
