using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Update.Process
{
    public class SolutionsRestore
    {
        private readonly IFileRestoreCommand _fileRestoreCommand;

        public SolutionsRestore(IFileRestoreCommand fileRestoreCommand)
        {
            _fileRestoreCommand = fileRestoreCommand;
        }

        public async Task CheckRestore(IEnumerable<PackageUpdateSet> targetUpdates, IFolder workingFolder, NuGetSources sources)
        {
            if (AnyProjectRequiresNuGetRestore(targetUpdates))
            {
                await Restore(workingFolder, sources);
            }
        }

        private async Task Restore(IFolder workingFolder, NuGetSources sources)
        {
            var solutionFiles = workingFolder.Find("*.sln");

            foreach (var sln in solutionFiles)
            {
                await _fileRestoreCommand.Invoke(sln, sources);
            }
        }

        private static bool AnyProjectRequiresNuGetRestore(IEnumerable<PackageUpdateSet> targetUpdates)
        {
            return targetUpdates.SelectMany(u => u.CurrentPackages)
                .Any(p => p.Path.PackageReferenceType != PackageReferenceType.ProjectFile);
        }
    }
}

