using System.Threading.Tasks;
using NuKeeper.NuGet.Process;
using NuKeeper.Types.Files;

namespace NuKeeper.Engine
{
    public class SolutionsRestore
    {
        private readonly IFileRestoreCommand _fileRestoreCommand;

        public SolutionsRestore(IFileRestoreCommand fileRestoreCommand)
        {
            _fileRestoreCommand = fileRestoreCommand;
        }

        public async Task Restore(IFolder workingFolder)
        {
            var solutionFiles = workingFolder.Find("*.sln");

            foreach (var sln in solutionFiles)
            {
                await _fileRestoreCommand.Invoke(sln);
            }
        }
    }
}

