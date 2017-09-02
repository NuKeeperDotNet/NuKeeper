using System.IO;
using System.Threading.Tasks;

namespace NuKeeper.NuGet.Process
{
    public interface IFileRestoreCommand
    {
        Task Invoke(FileInfo file);
    }
}
