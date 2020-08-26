using System.IO;

namespace NuKeeper.Abstractions.Configuration
{
    public class FileSettingsCache : IFileSettingsCache
    {
        private readonly IFileSettingsReader _reader;
        private FileSettings _settings;

        public FileSettingsCache(IFileSettingsReader reader)
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
