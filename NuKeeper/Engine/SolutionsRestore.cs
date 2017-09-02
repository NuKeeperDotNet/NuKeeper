using System.Threading.Tasks;
using NuKeeper.Files;
using NuKeeper.NuGet.Process;

namespace NuKeeper.Engine
{
    public class SolutionsRestore
    {
        private readonly ISolutionRestoreCommand _solutionRestoreCommand;

        public SolutionsRestore(ISolutionRestoreCommand solutionRestoreCommand)
        {
            _solutionRestoreCommand = solutionRestoreCommand;
        }

        public async Task Restore(IFolder workingFolder)
        {
            var solutionFiles = workingFolder.Find("*.sln");

            foreach (var sln in solutionFiles)
            {
                await _solutionRestoreCommand.Restore(sln);
            }
        }
    }
}

