using System.IO;
using System.Threading.Tasks;

namespace NuKeeper.NuGet.Process
{
    public interface IFileRestoreCommand : IPackageCommand
    {
        Task Invoke(FileInfo file);
    }
}
