using System.Collections.Generic;
using System.IO;

namespace NuKeeper.Types.Files
{
    public  interface IFolder
    {
        string FullPath { get; }
        void TryDelete();
        IEnumerable<FileInfo> Find(string pattern);
    }
}
