using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;
using SimpleInjector;

namespace NuKeeper
{
    public static class ContainerRegistration
    {
        public static Container Init(Settings settings)
        {
            var container = new Container();

            container.Register(() => settings, Lifestyle.Singleton);
            container.Register<ILogger, Logging.ConsoleLogger>();
            container.Register<IGithub, OctokitClient>();
            container.Register<IGithubRepositoryDiscovery, GithubRepositoryDiscovery>();
            container.Register<IPackageUpdateSelection, PackageUpdateSelection>();
            container.Register<IPackageUpdatesLookup, PackageUpdatesLookup>();
            container.Register<IBulkPackageLookup, BulkPackageLookup>();
            container.Register<IApiPackageLookup, ApiPackageLookup>();

            return container;
        }
    }
}
