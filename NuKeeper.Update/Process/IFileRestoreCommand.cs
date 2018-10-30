using System.IO;
using System.Threading.Tasks;
using NuKeeper.Abstractions.NuGet;

namespace NuKeeper.Update.Process
{
    public interface IFileRestoreCommand : IPackageCommand
    {
        Task Invoke(FileInfo file, NuGetSources sources);
    }
}
