using NuGet.Common;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.Engine.Report;
using NuKeeper.Github;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Logging;
using NuKeeper.NuGet.Process;
using NuKeeper.ProcessRunner;
using NuKeeper.Types.Files;
using NuKeeper.Types.Logging;
using SimpleInjector;

namespace NuKeeper
{
    public static class ContainerRegistration
    {
        public static Container Init(SettingsContainer settings)
        {
            var container = new Container();

            container.Register(() => settings.ModalSettings, Lifestyle.Singleton);
            container.Register(() => settings.GithubAuthSettings, Lifestyle.Singleton);
            container.Register(() => settings.UserSettings, Lifestyle.Singleton);

            container.Register<INuKeeperLogger, ConsoleLogger>();
            container.Register<ILogger, NuGetLogger>();

            container.Register<IFolder, Folder>();
            container.Register<IFolderFactory, FolderFactory>();
            container.Register<IFileRestoreCommand, NuGetFileRestoreCommand>();
            container.Register<IExternalProcess, ExternalProcess>();

            container.Register<IGithub, OctokitClient>();
            container.Register<IGithubRepositoryDiscovery, GithubRepositoryDiscovery>();
            container.Register<IPackageUpdateSelection, PackageUpdateSelection>();
            container.Register<IExistingBranchFilter, ExistingBranchFilter>();
            container.Register<IPackageUpdatesLookup, PackageUpdatesLookup>();
            container.Register<IBulkPackageLookup, BulkPackageLookup>();
            container.Register<IPackageVersionsLookup, PackageVersionsLookup>();
            container.Register<IApiPackageLookup, ApiPackageLookup>();
            container.Register<IRepositoryScanner, RepositoryScanner>();

            container.Register<GithubEngine>();
            container.Register<IGithubRepositoryEngine, GithubRepositoryEngine>();
            container.Register<IRepositoryUpdater, RepositoryUpdater>();
            container.Register<IPackageUpdater, PackageUpdater>();
            container.Register<IReportStreamSource, ReportStreamSource>();
            container.Register<IAvailableUpdatesReporter, AvailableUpdatesReporter>();
            container.Register<IForkFinder, ForkFinder>();

            return container;
        }
    }
}
