using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuKeeper.Files;

namespace NuKeeper.Engine.FilesUpdate
{
    public class ConfigFileFinder : IConfigFileFinder
    {
        private static readonly List<string> configFileNames = new List<string>
        {
            "web.config",
            "web.config.razor",
            "app.config",
            "app.config.razor",
        };

        public List<FileInfo> FindInFolder(IFolder workingFolder)
        {
            return configFileNames
                .SelectMany(fileName => workingFolder.Find(fileName))
                .ToList();
        }
    }
}
