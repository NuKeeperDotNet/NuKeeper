using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.Github;
using SimpleInjector;

namespace NuKeeper
{
    public static class ContainerRegistration
    {
        public static Container Init(SettingsContainer settings)
        {
            var container = new Container();

            Register(container, settings);
            ContainerInspectionRegistration.Register(container, settings);
            ContainerUpdateRegistration.Register(container, settings);

            return container;
        }

        private static void Register(Container container, SettingsContainer settings)
        {
            container.Register(() => settings.ModalSettings, Lifestyle.Singleton);
            container.Register(() => settings.GithubAuthSettings, Lifestyle.Singleton);
            container.Register(() => settings.UserSettings, Lifestyle.Singleton);

            container.Register<IGithub, OctokitClient>();
            container.Register<IGithubRepositoryDiscovery, GithubRepositoryDiscovery>();
            container.Register<IPackageUpdateSelection, PackageUpdateSelection>();
            container.Register<IExistingBranchFilter, ExistingBranchFilter>();

            container.Register<GithubEngine>();
            container.Register<IGithubRepositoryEngine, GithubRepositoryEngine>();
            container.Register<IRepositoryUpdater, RepositoryUpdater>();

            container.Register<ILocalUpdater, LocalUpdater>();
            container.Register<IPackageUpdater, PackageUpdater>();
            container.Register<IForkFinder, ForkFinder>();
        }
    }
}
