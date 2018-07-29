using NuKeeper.Creators;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.GitHub;
using NuKeeper.Local;
using NuKeeper.Update.Selection;
using SimpleInjector;

namespace NuKeeper
{
    public static class CreatorRegistrations
    {
        public static void Initialize(Container container)
        {
            container.Register<ICreate<IGitHub>, OctokitClientCreator>();
            container.Register<ICreate<IGitHubRepositoryDiscovery>, GitHubRepositoryDiscoveryCreator>();
            container.Register<ICreate<IExistingBranchFilter>, ExistingBranchFilterCreator>();
            container.Register<ICreate<IPackageUpdateSelection>, PackageUpdateSelectionCreator>();
            container.Register<ICreate<IRepositoryUpdater>, RepositoryUpdaterCreator>();
            container.Register<ICreate<IGitHubRepositoryEngine>, GitHubRepositoryEngineCreator>();
            container.Register<ICreate<IRepositoryFilter>, RepositoryFilterCreator>();
            container.Register<ICreate<IForkFinder>, ForkFinderCreator>();
            container.Register<ICreate<IPackageUpdater>, PackageUpdaterCreator>();
            container.Register<ICreate<IUpdateSelection>, UpdateSelectionCreator>();
            container.Register<ICreate<ILocalUpdater>, LocalUpdaterCreator>();
        }
    }
}
