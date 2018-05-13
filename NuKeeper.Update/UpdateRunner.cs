using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Update.Process;

namespace NuKeeper.Update
{
    public class UpdateRunner : IUpdateRunner
    {
        private readonly INuKeeperLogger _logger;
        private readonly NuGetSources _sources;

        public UpdateRunner(
            INuKeeperLogger logger,
            NuGetSources sources)
        {
            _logger = logger;
            _sources = sources;
        }

        public async Task Update(PackageUpdateSet updateSet)
        {
            foreach (var current in updateSet.CurrentPackages)
            {
                var updateCommands = GetUpdateCommands(current.Path.PackageReferenceType);
                foreach (var updateCommand in updateCommands)
                {
                    await updateCommand.Invoke(updateSet.SelectedVersion, updateSet.Selected.Source, current);
                }
            }
        }

        private IReadOnlyCollection<IPackageCommand> GetUpdateCommands(
            PackageReferenceType packageReferenceType)
        {
            switch (packageReferenceType)
            {
                case PackageReferenceType.PackagesConfig:
                    return new IPackageCommand[]
                    {
                        new NuGetFileRestoreCommand(_logger, _sources),
                        new NuGetUpdatePackageCommand(_logger, _sources)
                    };

                case PackageReferenceType.ProjectFileOldStyle:
                    return new IPackageCommand[]
                    {
                        new UpdateProjectImportsCommand(),
                        new NuGetFileRestoreCommand(_logger, _sources),
                        new DotNetUpdatePackageCommand(_logger, _sources)
                    };

                case PackageReferenceType.ProjectFile:
                    return new[] {new DotNetUpdatePackageCommand(_logger, _sources) };

                case PackageReferenceType.Nuspec:
                    return new[] { new UpdateNuspecCommand(_logger) };

                default:
                    throw new ArgumentOutOfRangeException(nameof(packageReferenceType));
            }
        }
    }
}
