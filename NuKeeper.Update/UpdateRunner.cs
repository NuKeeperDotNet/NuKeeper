using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.Process;

namespace NuKeeper.Update
{
    public class UpdateRunner : IUpdateRunner
    {
        private readonly INuKeeperLogger _logger;

        public UpdateRunner(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public async Task Update(PackageUpdateSet updateSet, NuGetSources sources)
        {
            foreach (var current in updateSet.CurrentPackages)
            {
                var updateCommands = GetUpdateCommands(current.Path.PackageReferenceType, sources);
                foreach (var updateCommand in updateCommands)
                {
                    await updateCommand.Invoke(current, updateSet.SelectedVersion, updateSet.Selected.Source, sources);
                }
            }
        }

        private IReadOnlyCollection<IPackageCommand> GetUpdateCommands(
            PackageReferenceType packageReferenceType,
            NuGetSources sources)
        {
            switch (packageReferenceType)
            {
                case PackageReferenceType.PackagesConfig:
                    return new IPackageCommand[]
                    {
                        new NuGetFileRestoreCommand(_logger),
                        new NuGetUpdatePackageCommand(_logger)
                    };

                case PackageReferenceType.ProjectFileOldStyle:
                    return new IPackageCommand[]
                    {
                        new UpdateProjectImportsCommand(),
                        new NuGetFileRestoreCommand(_logger),
                        new DotNetUpdatePackageCommand(_logger)
                    };

                case PackageReferenceType.ProjectFile:
                    return new[] {new DotNetUpdatePackageCommand(_logger) };

                case PackageReferenceType.Nuspec:
                    return new[] { new UpdateNuspecCommand(_logger) };

                default:
                    throw new ArgumentOutOfRangeException(nameof(packageReferenceType));
            }
        }
    }
}
