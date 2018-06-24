using System.IO;
using System.Threading.Tasks;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Update.Process
{
    public interface IFileRestoreCommand : IPackageCommand
    {
        Task Invoke(FileInfo file, NuGetSources sources);
    }
}
