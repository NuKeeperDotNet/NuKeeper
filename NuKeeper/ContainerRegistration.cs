using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;
using NuKeeper.AzureDevOps;
using NuKeeper.BitBucket;
using NuKeeper.BitBucketLocal;
using NuKeeper.Collaboration;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.Git;
using NuKeeper.Gitea;
using NuKeeper.GitHub;
using NuKeeper.Gitlab;
using NuKeeper.Local;
using NuKeeper.Update.Selection;
using SimpleInjector;
using System.Linq;
using System.Reflection;
using NuKeeper.Commands;
using NuKeeper.Update.Process;

namespace NuKeeper
{
    public static class ContainerRegistration
    {
        public static Container Init()
        {
            var container = new Container();

            Register(container);
            RegisterCommands(container);
            ContainerInspectionRegistration.Register(container);
            ContainerUpdateRegistration.Register(container);

            return container;
        }

        private static void RegisterCommands(Container container)
        {
            container.Register<GlobalCommand>();
            container.Register<InspectCommand>();
            container.Register<OrganisationCommand>();
            container.Register<RepositoryCommand>();
            container.Register<UpdateCommand>();
        }

        private static void Register(Container container)
        {
            container.Register<ILocalEngine, LocalEngine>();
            container.Register<ICollaborationEngine, CollaborationEngine>();
            container.Register<IGitRepositoryEngine, GitRepositoryEngine>();
            container.Register<IRepositoryUpdater, RepositoryUpdater>();
            container.Register<IPackageUpdateSelection, PackageUpdateSelection>();
            container.Register<IExistingCommitFilter, ExistingCommitFilter>();
            container.Register<IPackageUpdater, PackageUpdater>();
            container.Register<IRepositoryFilter, RepositoryFilter>();
            container.Register<ISolutionRestore, SolutionRestore>();

            container.Register<ILocalUpdater, LocalUpdater>();
            container.Register<IUpdateSelection, UpdateSelection>();
            container.Register<IFileSettingsCache, FileSettingsCache>();
            container.Register<IFileSettingsReader, FileSettingsReader>();

            container.RegisterSingleton<IEnvironmentVariablesProvider, EnvironmentVariablesProvider>();

            container.RegisterSingleton<IGitDiscoveryDriver, LibGit2SharpDiscoveryDriver>();

            container.RegisterSingleton<ICollaborationFactory, CollaborationFactory>();

            var settingsRegistration = RegisterMultipleSingletons<ISettingsReader>(container, new[]
            {
                typeof(GitHubSettingsReader).Assembly,
                typeof(AzureDevOpsSettingsReader).Assembly,
                typeof(VstsSettingsReader).Assembly,
                typeof(BitbucketSettingsReader).Assembly,
                typeof(BitBucketLocalSettingsReader).Assembly,
                typeof(GitlabSettingsReader).Assembly,
                typeof(GiteaSettingsReader).Assembly
            });

            container.Collection.Register<ISettingsReader>(settingsRegistration);
        }

        private static Registration[] RegisterMultipleSingletons<T>(Container container, Assembly[] assemblies)
        {
            var types = container.GetTypesToRegister(typeof(T), assemblies);

            return (from type in types
                    select Lifestyle.Singleton.CreateRegistration(type, container)
            ).ToArray();
        }
    }
}
