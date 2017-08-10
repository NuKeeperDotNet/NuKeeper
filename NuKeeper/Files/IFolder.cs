using System.Collections.Generic;
using System.IO;

namespace NuKeeper.Files
{
    public  interface IFolder
    {
        string FullPath { get; }
        IList<FileInfo> FindFiles(string pattern);
        void TryDelete();
    }
}