using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.NuGet.Process;

namespace NuKeeper.Engine.Packages
{
    public class LocalPackageUpdater : ILocalPackageUpdater
    {
        private readonly INuKeeperLogger _logger;
        private readonly UserSettings _settings;

        public LocalPackageUpdater(
            INuKeeperLogger logger,
            UserSettings settings)
        {
            _logger = logger;
            _settings = settings;
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

        private IReadOnlyCollection<IPackageCommand> GetUpdateCommands(PackageReferenceType packageReferenceType)
        {
            switch (packageReferenceType)
            {
                case PackageReferenceType.PackagesConfig:
                    return new IPackageCommand[]
                    {
                        new NuGetFileRestoreCommand(_logger, _settings),
                        new NuGetUpdatePackageCommand(_logger, _settings)
                    };

                case PackageReferenceType.ProjectFileOldStyle:
                    return new IPackageCommand[]
                    {
                        new UpdateProjectImportsCommand(),
                        new NuGetFileRestoreCommand(_logger, _settings),
                        new DotNetUpdatePackageCommand(_logger, _settings)
                    };

                case PackageReferenceType.ProjectFile:
                    return new[] {new DotNetUpdatePackageCommand(_logger, _settings)};

                case PackageReferenceType.Nuspec:
                    return new[] { new UpdateNuspecCommand(_logger) };

                default: throw new ArgumentOutOfRangeException(nameof(packageReferenceType));
            }
        }
    }
}
