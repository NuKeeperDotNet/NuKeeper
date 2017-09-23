﻿using NuGet.Common;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Files;
using NuKeeper.Github;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;
using NuKeeper.NuGet.Process;
using NuKeeper.ProcessRunner;
using NuKeeper.RepositoryInspection;
using SimpleInjector;

namespace NuKeeper
{
    public static class ContainerRegistration
    {
        public static Container Init(Settings settings)
        {
            var container = new Container();

            container.Register(() => settings, Lifestyle.Singleton);
            container.Register<INuKeeperLogger, ConsoleLogger>();
            container.Register<ILogger, NuGetLogger>();

            container.Register<IFolder, Folder>();
            container.Register<IFolderFactory, FolderFactory>();
            container.Register<IFileRestoreCommand, NuGetFileRestoreCommand>();
            container.Register<IExternalProcess, ExternalProcess>();

            container.Register<IGithub, OctokitClient>();
            container.Register<IGithubRepositoryDiscovery, GithubRepositoryDiscovery>();
            container.Register<IPackageUpdateSelection, PackageUpdateSelection>();
            container.Register<IPackageUpdatesLookup, PackageUpdatesLookup>();
            container.Register<IBulkPackageLookup, BulkPackageLookup>();
            container.Register<IPackageVersionsLookup, PackageVersionsLookup>();
            container.Register<IApiPackageLookup, ApiPackageLookup>();
            container.Register<IRepositoryScanner, RepositoryScanner>();

            container.Register<GithubEngine>();
            container.Register<IRepositoryUpdater, RepositoryUpdater>();
            container.Register<IPackageUpdater, PackageUpdater>();

            return container;
        }
    }
}
