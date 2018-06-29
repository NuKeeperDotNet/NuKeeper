using NuGet.Common;
using SimpleInjector;
using NuKeeper.Configuration;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using NuKeeper.Inspection.Sources;

namespace NuKeeper
{
    public static class ContainerInspectionRegistration
    {
        public static void Register(Container container, SettingsContainer settings)
        {
            container.RegisterInstance<INuKeeperLogger>(
                new ConsoleLogger(settings.UserSettings.LogLevel));

            container.Register<ILogger, NuGetLogger>();

            container.Register<IFolder, Folder>();
            container.Register<IFolderFactory, FolderFactory>();

            container.Register<IPackageUpdatesLookup, PackageUpdatesLookup>();
            container.Register<IBulkPackageLookup, BulkPackageLookup>();
            container.Register<IPackageVersionsLookup, PackageVersionsLookup>();
            container.Register<IApiPackageLookup, ApiPackageLookup>();
            container.Register<IRepositoryScanner, RepositoryScanner>();

            container.Register<IUpdateFinder, UpdateFinder>();
            container.Register<INuGetSourcesReader, NuGetSourcesReader>();

            container.Register<IReportStreamSource, ReportStreamSource>();
            container.Register<IAvailableUpdatesReporter, CsvFileReporter>();
            container.Register<IPackageUpdateSetSort, PackageUpdateSetSort>();
        }
    }
}
