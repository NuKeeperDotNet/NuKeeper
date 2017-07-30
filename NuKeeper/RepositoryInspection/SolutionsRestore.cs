using System.Threading.Tasks;
using NuKeeper.NuGet.Process;

namespace NuKeeper.RepositoryInspection
{
    public class SolutionsRestore
    {
        private readonly ISolutionRestoreCommand _solutionRestoreCommand;

        public SolutionsRestore(ISolutionRestoreCommand solutionRestoreCommand)
        {
            _solutionRestoreCommand = solutionRestoreCommand;
        }

        public async Task Restore(string rootDirectory)
        {
            var solutionFiles = SolutionFinder.FindSolutions(rootDirectory);

            foreach (var solutionPath in solutionFiles)
            {
                await _solutionRestoreCommand.Restore(solutionPath);
            }
        }
    }
}