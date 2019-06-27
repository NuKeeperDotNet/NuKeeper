using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using NuKeeper.Update.Process;

namespace NuKeeper.Update
{
    public class UpdateRunner : IUpdateRunner
    {
        private readonly INuKeeperLogger _logger;
        private readonly IFileRestoreCommand _fileRestoreCommand;
        private readonly INuGetUpdatePackageCommand _nuGetUpdatePackageCommand;
        private readonly IDotNetUpdatePackageCommand _dotNetUpdatePackageCommand;
        private readonly IDotNetRestoreCommand _dotNetRestoreCommand;
        private readonly IUpdateProjectImportsCommand _updateProjectImportsCommand;
        private readonly IUpdateNuspecCommand _updateNuspecCommand;
        private readonly IUpdateDirectoryBuildTargetsCommand _updateDirectoryBuildTargetsCommand;

        public UpdateRunner(
            INuKeeperLogger logger,
            IFileRestoreCommand fileRestoreCommand,
            INuGetUpdatePackageCommand nuGetUpdatePackageCommand,
            IDotNetUpdatePackageCommand dotNetUpdatePackageCommand,
            IDotNetRestoreCommand dotNetRestoreCommand,
            IUpdateProjectImportsCommand updateProjectImportsCommand,
            IUpdateNuspecCommand updateNuspecCommand,
            IUpdateDirectoryBuildTargetsCommand updateDirectoryBuildTargetsCommand)
        {
            _logger = logger;
            _fileRestoreCommand = fileRestoreCommand;
            _nuGetUpdatePackageCommand = nuGetUpdatePackageCommand;
            _dotNetUpdatePackageCommand = dotNetUpdatePackageCommand;
            _dotNetRestoreCommand = dotNetRestoreCommand;
            _updateProjectImportsCommand = updateProjectImportsCommand;
            _updateNuspecCommand = updateNuspecCommand;
            _updateDirectoryBuildTargetsCommand = updateDirectoryBuildTargetsCommand;
        }

        public async Task Update(PackageUpdateSet updateSet, NuGetSources sources)
        {
            var sortedUpdates = Sort(updateSet.CurrentPackages);

            _logger.Detailed($"Updating '{updateSet.SelectedId}' to {updateSet.SelectedVersion} in {sortedUpdates.Count} projects");

            foreach (var current in sortedUpdates)
            {
                // TODO: Find a way to access UserSettings.RestoreBeforePackageUpdate here. Should be passed to GetUpdateCommands function.
                var updateCommands = GetUpdateCommands(current.Path.PackageReferenceType, false);
                foreach (var updateCommand in updateCommands)
                {
                    await updateCommand.Invoke(current,
                        updateSet.SelectedVersion, updateSet.Selected.Source,
                        sources);
                }
            }

            var projectsWithUpdateRequiringRestore = PackageProjectsRequiringDotNetRestore(sortedUpdates);
            foreach (var projectRequiringRestore in projectsWithUpdateRequiringRestore)
            {
                await _dotNetRestoreCommand.Invoke(projectRequiringRestore, sources);
            }
        }

        private IReadOnlyCollection<PackageInProject> Sort(IReadOnlyCollection<PackageInProject> packages)
        {
            var sorter = new PackageInProjectTopologicalSort(_logger);
            return sorter.Sort(packages)
                .ToList();
        }

        private IReadOnlyCollection<IPackageCommand> GetUpdateCommands(
            PackageReferenceType packageReferenceType, bool restoreBeforePackageUpdate)
        {
            var commands = new List<IPackageCommand>();

            switch (packageReferenceType)
            {
                case PackageReferenceType.PackagesConfig:
                    commands.Add(_fileRestoreCommand);
                    commands.Add(_nuGetUpdatePackageCommand);
                    break;
                case PackageReferenceType.ProjectFileOldStyle:
                    commands.Add(_updateProjectImportsCommand);
                    commands.Add(_fileRestoreCommand);
                    commands.AddRange(GetDotNetUpdatePackageCommand(restoreBeforePackageUpdate));
                    break;
                case PackageReferenceType.ProjectFile:
                    commands.AddRange(GetDotNetUpdatePackageCommand(restoreBeforePackageUpdate));
                    break;
                case PackageReferenceType.Nuspec:
                    commands.Add(_updateNuspecCommand);
                    break;
                case PackageReferenceType.DirectoryBuildTargets:
                    commands.Add(_updateDirectoryBuildTargetsCommand);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(packageReferenceType));
            }

            return commands.ToArray();
        }

        private IReadOnlyCollection<IPackageCommand> GetDotNetUpdatePackageCommand(bool restoreBeforePackageUpdate)
        {
            var commands = new List<IPackageCommand>();

            if (restoreBeforePackageUpdate)
            {
                commands.Add(_dotNetRestoreCommand);
            }

            commands.Add(_dotNetUpdatePackageCommand);
            return commands.ToArray();
        }

        private static IEnumerable<PackageInProject> PackageProjectsRequiringDotNetRestore(IReadOnlyCollection<PackageInProject> packagesInProjects)
        {
            return packagesInProjects.Where(
                x => x.Path.PackageReferenceType == PackageReferenceType.ProjectFileOldStyle
                || x.Path.PackageReferenceType == PackageReferenceType.ProjectFile);
        }
    }
}
