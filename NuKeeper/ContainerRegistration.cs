using NuKeeper.Abstract;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Abstract.Engine;
using NuKeeper.Abstract.Engine.Packages;
using NuKeeper.Abstract.Local;
using NuKeeper.Engine;
using NuKeeper.Github.Configuration;
using NuKeeper.Github.Engine;
using NuKeeper.GitHub;
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
            container.Register<IEngine, GitHubEngine>();
            container.Register<IRepositoryEngine, RepositoryEngine>();
            container.Register<IGitHubRepositoryDiscovery, GitHubRepositoryDiscovery>();
            container.Register<IRepositoryUpdater, RepositoryUpdater>();
            container.Register<IPackageUpdateSelection, PackageUpdateSelection>();
            container.Register<IExistingBranchFilter, ExistingBranchFilter>();
            container.Register<IPackageUpdater, PackageUpdater>();
            container.Register<IForkFinder, GithubForkFinder>();
            container.Register<IRepositoryFilter, GithubRepositoryFilter>();
            container.Register<ILocalUpdater, LocalUpdater>();
            container.Register<IUpdateSelection, UpdateSelection>();
            container.Register<IFileSettingsCache, FileSettingsCache>();
            container.Register<ISettingsReader, GitSettingsReader>();
            container.RegisterSingleton<IClient, OctokitClient>();
        }
    }
}
