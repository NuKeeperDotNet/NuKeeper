using System.Linq;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.AzureDevOps;
using NuKeeper.BitBucket;
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
            
            
            
            var settingsReaders = container.GetTypesToRegister(typeof(ISettingsReader), new[]
            {
                typeof(GitHubSettingsReader).Assembly,
                typeof(AzureDevOpsSettingsReader).Assembly,
                typeof(BitbucketSettingsReader).Assembly,
            });

            var settingsRegistration = (
                from type in settingsReaders
                select Lifestyle.Singleton.CreateRegistration(type, container)
            ).ToArray(); 
            
            container.Collection.Register<ISettingsReader>(settingsRegistration);
        }
    }
}
