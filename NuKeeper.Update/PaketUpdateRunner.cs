using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using Paket;

namespace NuKeeper.Update
{
    public class PaketUpdateRunner : IUpdateRunner
    {
        private INuKeeperLogger _logger;

        public PaketUpdateRunner(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public Task Update(PackageUpdateSet updateSet, NuGetSources sources, IFolder folder)
        {
            var sortedUpdates = Sort(updateSet.CurrentPackages);
            var dependencies = new Dependencies($"{folder.FullPath}/paket.dependencies");
            _logger.Detailed($"Updating '{updateSet.SelectedId}' to {updateSet.SelectedVersion} in {sortedUpdates.Count} projects");

            foreach (var current in sortedUpdates)
            {
                dependencies.UpdatePackage(FSharpOption<string>.None, current.Id, FSharpOption<string>.Some(updateSet.SelectedVersion.Version.ToString()), false, SemVerUpdateMode.NoRestriction,false);
            }

            return Task.CompletedTask;
        }

        private IReadOnlyCollection<PackageInProject> Sort(IReadOnlyCollection<PackageInProject> packages)
        {
            var sorter = new PackageInProjectTopologicalSort(_logger);
            return sorter.Sort(packages)
                .ToList();
        }
    }
}
