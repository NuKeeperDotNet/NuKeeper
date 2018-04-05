using System.Collections.Generic;
using System.IO;

namespace NuKeeper.Engine.FilesUpdate
{
    public interface IConfigFileFinder
    {
        IEnumerable<FileInfo> FindConfigFiles();
    }
}
