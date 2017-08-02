using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
using NuKeeper.NuGet.Api;
using SimpleInjector;

namespace NuKeeper
{
    public static class Registry
    {
        public static Container RegisterContainer(Settings settings)
        {
            var container = new Container();

            container.Register(() => settings, Lifestyle.Singleton);
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
