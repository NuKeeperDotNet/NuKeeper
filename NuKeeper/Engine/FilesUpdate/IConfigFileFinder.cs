using System.Collections.Generic;
using System.IO;
using NuKeeper.Files;

namespace NuKeeper.Engine.FilesUpdate
{
    public interface IConfigFileFinder
    {
        IEnumerable<FileInfo> FindInFolder(IFolder workingFolder);
    }
}
