using System.Collections.Generic;
using System.IO;

namespace NuKeeper.Abstractions.Inspections.Files
{
    public  interface IFolder
    {
        string FullPath { get; }
        void TryDelete();
        IReadOnlyCollection<FileInfo> Find(string pattern);
    }
}
