using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuKeeper.Files;

namespace NuKeeper.Engine.FilesUpdate
{
    public class ConfigFileFinder
    {
        private static readonly List<string> configFileNames = new List<string>
        {
            "web.config",
            "web.config.razor",
            "app.config",
            "app.config.razor",
        };

        private readonly IFolder _workingFolder;

        public ConfigFileFinder(IFolder workingFolder)
        {
            _workingFolder = workingFolder;
        }

        public IEnumerable<FileInfo> FindConfigFiles()
        {
            return configFileNames.SelectMany
                (fileName => _workingFolder.Find(fileName));
        }
    }
}
