using System.Linq;
using NuGet.Common;
using NuKeeper.Configuration;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using SimpleInjector;

namespace NuKeeper
{
    public static class ContainerInpectionRegistration
    {
        public static void Register(Container container, SettingsContainer settings)
        {
            var packageLookupSettings = new PackageUpdateLookupSettings
            {
                AllowedChange = settings.UserSettings.AllowedChange,
                NugetSources = settings.UserSettings.NuGetSources?.ToList()
            };

            container.RegisterInstance(packageLookupSettings);

            container.Register<ILogger, NuGetLogger>();

            container.Register<IFolder, Folder>();
            container.Register<IFolderFactory, FolderFactory>();

            container.Register<IPackageUpdatesLookup, PackageUpdatesLookup>();
            container.Register<IBulkPackageLookup, BulkPackageLookup>();
            container.Register<IPackageVersionsLookup, PackageVersionsLookup>();
            container.Register<IApiPackageLookup, ApiPackageLookup>();
            container.Register<IRepositoryScanner, RepositoryScanner>();

            container.Register<IUpdateFinder, UpdateFinder>();
        }
    }
}
