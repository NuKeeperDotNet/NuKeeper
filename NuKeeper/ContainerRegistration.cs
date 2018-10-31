using NuKeeper.Abstractions.Configuration;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.GitHub;
using NuKeeper.Local;
using NuKeeper.Update.Selection;
using SimpleInjector;

namespace NuKeeper
{
    public static class ContainerRegistration
    {
        public static Container Init()
        {
            var container = new Container();

            Register(container);
            ContainerInspectionRegistration.Register(container);
            ContainerUpdateRegistration.Register(container);

            return container;
        }

        private static void Register(Container container)
        {
            container.Register<ILocalEngine, LocalEngine>();
            container.Register<IGitHubEngine, GitHubEngine>();
            container.Register<IGitHubRepositoryEngine, GitHubRepositoryEngine>();
            container.Register<IGitHubRepositoryDiscovery, GitHubRepositoryDiscovery>();
            container.Register<IRepositoryUpdater, RepositoryUpdater>();
            container.Register<IPackageUpdateSelection, PackageUpdateSelection>();
            container.Register<IExistingBranchFilter, ExistingBranchFilter>();
            container.Register<IPackageUpdater, PackageUpdater>();
            container.Register<IForkFinder, ForkFinder>();
            container.Register<IRepositoryFilter, RepositoryFilter>();

            container.Register<ILocalUpdater, LocalUpdater>();
            container.Register<IUpdateSelection, UpdateSelection>();
            container.Register<IFileSettingsCache, FileSettingsCache>();

            container.RegisterSingleton<IGitHub, OctokitClient>();
        }
    }
}
