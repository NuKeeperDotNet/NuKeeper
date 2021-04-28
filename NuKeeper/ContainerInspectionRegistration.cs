using System.Text;
using SimpleInjector;
using NuGet.Common;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Engine;
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
    internal static class ContainerInspectionRegistration
    {
        internal static void Register(Container container)
        {
            var logger = new ConfigurableLogger();
            container.RegisterInstance<INuKeeperLogger>(logger);
            container.RegisterInstance<IConfigureLogger>(logger);
            container.Register<ILogger, NuGetLogger>();

            container.Register<IDirectoryExclusions, DirectoryExclusions>();
            container.Register<IFolderFactory, FolderFactory>();

            container.Register<IPackageUpdatesLookup, PackageUpdatesLookup>();
            container.Register<IBulkPackageLookup, BulkPackageLookup>();
            container.Register<IPackageLookupResultReporter, PackageLookupResultReporter>();
            container.Register<IPackageVersionsLookup, PackageVersionsLookup>();
            container.Register<IApiPackageLookup, ApiPackageLookup>();
            container.Register<IRepositoryScanner, RepositoryScanner>();
            container.Register<INameTemplateInterpolater, NameTemplateInterpolater>();

            container.Register<ProjectFileReader>();
            container.Register<PackagesFileReader>();
            container.Register<NuspecFileReader>();
            container.Register<DirectoryBuildTargetsReader>();

            container.Register<IUpdateFinder, UpdateFinder>();
            container.Register<INuGetSourcesReader, NuGetSourcesReader>();
            container.Register<INuGetConfigFileReader, NuGetConfigFileReader>();

            container.Register<IReporter, Reporter>();
            container.Register<IPackageUpdateSetSort, PackageUpdateSetSort>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
