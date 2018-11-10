using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.AzureDevOps;
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
            container.Register<ICollaborationEngine, CollaborationEngine>();
            container.Register<IGitRepositoryEngine, GitRepositoryEngine>();
            container.Register<IRepositoryUpdater, RepositoryUpdater>();
            container.Register<IPackageUpdateSelection, PackageUpdateSelection>();
            container.Register<IExistingBranchFilter, ExistingBranchFilter>();
            container.Register<IPackageUpdater, PackageUpdater>();
            container.Register<IRepositoryFilter, RepositoryFilter>();

            container.Register<ILocalUpdater, LocalUpdater>();
            container.Register<IUpdateSelection, UpdateSelection>();
            container.Register<IFileSettingsCache, FileSettingsCache>();

            container.RegisterSingleton<ICollaborationFactory,CollaborationFactory>();
            container.Collection.Register<ISettingsReader>(typeof(GitHubSettingsReader), typeof(AzureDevOpsSettingsReader));

            //GitHub Registrations
            container.RegisterSingleton<ICollaborationPlatform, OctokitClient>();

            //Azure Registrations
            //container.RegisterSingleton<ICollaborationPlatform, AzureDevOpsPlatform>();
        }
    }
}
